using Common.SharedKernel.Application.Constants.Reports;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Reports.Application.LoadingInfo.Contracts;
using Reports.Application.LoadingInfo.Models;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.Products;

namespace Reports.Application.LoadingInfo.Features.Products;

public sealed class EtlProductsService : IProductsLoader
{
    private const string ExecutionName = "loading-info";

    private readonly IProductSheetReadRepository readRepository;
    private readonly IProductSheetWriteRepository writeRepository;
    private readonly IEtlExecutionRepository executionRepository;
    private readonly ILogger<EtlProductsService> logger;

    public EtlProductsService(
        IProductSheetReadRepository readRepository,
        IProductSheetWriteRepository writeRepository,
        IEtlExecutionRepository executionRepository,
        ILogger<EtlProductsService> logger)
    {
        this.readRepository = readRepository;
        this.writeRepository = writeRepository;
        this.executionRepository = executionRepository;
        this.logger = logger;
    }

    public async Task<Result<EtlExecutionMetrics>> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("ETLProductos iniciado.");

            // Obtener el último RowVersion exitoso
            var lastRowVersion = await executionRepository
                                         .GetLastSuccessfulExecutionTimestampAsync(ExecutionName, cancellationToken);

            if (lastRowVersion.HasValue)
            {
                logger.LogInformation(
                     "Carga incremental activada. LastRowVersion={LastRowVersion}",
                      lastRowVersion.Value);
            }
            else
            {
                logger.LogInformation("Carga completa (primera ejecución o no hay ejecuciones exitosas previas).");
                await writeRepository.TruncateAsync(cancellationToken);
            }

            var readRows = 0;
            var insertedRows = 0;
            var nextId = 1L;
            var fundIdsToDelete = new HashSet<int>();
            var allRows = new List<ProductSheet>();

            // Primero: Leer todos los datos y recolectar IDs
            await foreach (var row in readRepository.ReadProductsAsync(lastRowVersion, cancellationToken))
            {
                readRows++;
                fundIdsToDelete.Add(row.FundId);

                var createResult = ProductSheet.Create(
                   id: nextId++,
                   administratorId: row.AdministratorId,
                   entityType: row.EntityType,
                   entityCode: row.EntityCode,
                   entitySfcCode: row.EntitySfcCode,
                   businessCodeSfcFund: row.BusinessCodeSfcFund,
                    fundId: row.FundId);

                if (createResult.IsFailure || createResult.Value is null)
                {
                    return Result.Failure<EtlExecutionMetrics>(createResult.Error);
                }

                allRows.Add(createResult.Value);
            }

            // Segundo: Borrar registros existentes (solo en carga incremental)
            if (lastRowVersion.HasValue && fundIdsToDelete.Count > 0)
            {
                logger.LogInformation(
                    "Eliminando {Count} registros de productos existentes antes de insertar actualizaciones.",
               fundIdsToDelete.Count);
                await writeRepository.DeleteByFundIdsAsync(fundIdsToDelete, cancellationToken);
            }

            // Tercero: Insertar en batches
            var batch = new List<ProductSheet>(ReportsBulkProperties.EtlBatchSize);

            foreach (var row in allRows)
            {
                batch.Add(row);

                if (batch.Count >= ReportsBulkProperties.EtlBatchSize)
                {
                    await writeRepository.BulkInsertAsync(batch, cancellationToken);
                    insertedRows += batch.Count;
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await writeRepository.BulkInsertAsync(batch, cancellationToken);
                insertedRows += batch.Count;
            }

            logger.LogInformation(
              "ETLProductos finalizado. Leídas={ReadRows}, Insertadas={InsertedRows}",
             readRows, insertedRows);

            return Result.Success(new EtlExecutionMetrics(readRows, insertedRows));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ETLProductos falló.");
            return Result.Failure<EtlExecutionMetrics>(
             new Error("ETL_PRODUCTS_001", ex.Message, ErrorType.Failure));
        }
    }
}