using Common.SharedKernel.Application.Constants.Reports;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Reports.Application.LoadingInfo.Contracts;
using Reports.Application.LoadingInfo.Models;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.People;

namespace Reports.Application.LoadingInfo.Features.People;

public sealed class EtlClientMembershipService : IPeopleLoader
{
    private const string ExecutionName = "loading-info";

    private readonly IPeopleSheetReadRepository readRepository;
    private readonly IPeopleSheetWriteRepository writeRepository;
    private readonly IEtlExecutionRepository executionRepository;
    private readonly ILogger<EtlClientMembershipService> logger;

    public EtlClientMembershipService(
      IPeopleSheetReadRepository readRepository,
      IPeopleSheetWriteRepository writeRepository,
      IEtlExecutionRepository executionRepository,
      ILogger<EtlClientMembershipService> logger)
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
            logger.LogInformation("ETL ClientMembership (ETLafiliado_clientes) iniciado.");

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
            var memberIdsToDelete = new HashSet<long>();
            var allRows = new List<PeopleSheet>();

            // Primero: Leer todos los datos y recolectar IDs
            await foreach (var row in readRepository.ReadPeopleAsync(lastRowVersion, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                readRows++;
                memberIdsToDelete.Add(row.MemberId);

                var createResult = PeopleSheet.Create(
                  identificationType: row.IdentificationType,
              normalizedIdentificationType: row.IdentificationTypeHomologated,
                      affiliateId: row.MemberId,
                     identificationNumber: row.Identification,
                fullName: row.FullName,
                          birthDate: row.Birthday,
                gender: row.Gender);

                if (createResult.IsFailure)
                {
                    logger.LogWarning(
                     "Error creando fila PeopleSheet. MemberId={MemberId}. Error={Error}",
                    row.MemberId,
                         createResult.Error);

                    return Result.Failure<EtlExecutionMetrics>(createResult.Error);
                }

                allRows.Add(createResult.Value!);
            }

            // Segundo: Borrar registros existentes (solo en carga incremental)
            if (lastRowVersion.HasValue && memberIdsToDelete.Count > 0)
            {
                logger.LogInformation(
                 "Eliminando {Count} registros existentes antes de insertar actualizaciones.",
                        memberIdsToDelete.Count);
                await writeRepository.DeleteByMemberIdsAsync(memberIdsToDelete, cancellationToken);
            }

            // Tercero: Insertar en batches
            var batch = new List<PeopleSheet>(ReportsBulkProperties.EtlBatchSize);

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
           "ClientMembership (ETLafiliado_clientes) finalizado. FilasLeídas={ReadRows} FilasInsertadas={InsertedRows}",
            readRows, insertedRows);

            return Result.Success(new EtlExecutionMetrics(readRows, insertedRows));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ETLafiliado_clientes falló.");

            return Result.Failure<EtlExecutionMetrics>(
            new Error(
            "ETL_PEOPLE_001",
             $"ETLafiliado_clientes falló: {exception.Message}",
             ErrorType.Failure));
        }
    }
}