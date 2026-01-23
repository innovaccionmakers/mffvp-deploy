using Common.SharedKernel.Application.Constants.Reports;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Reports.Application.LoadingInfo.Contracts;
using Reports.Application.LoadingInfo.Models;
using Reports.Domain.LoadingInfo.Closing;

namespace Reports.Application.LoadingInfo.Features.Closing;

public sealed class EtlClosingService : IClosingLoader
{
    private readonly IClosingSheetReadRepository readRepository;
    private readonly IClosingSheetWriteRepository writeRepository;
    private readonly ILogger<EtlClosingService> logger;

    public EtlClosingService(
           IClosingSheetReadRepository readRepository,
           IClosingSheetWriteRepository writeRepository,
       ILogger<EtlClosingService> logger)
    {
        this.readRepository = readRepository;
        this.writeRepository = writeRepository;
        this.logger = logger;
    }

    public async Task<Result<EtlExecutionMetrics>> ExecuteAsync(
        DateTime closingDateUtc,
        int portfolioId,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("ETLClosing iniciado. ClosingDate={ClosingDate}, PortfolioId={PortfolioId}", closingDateUtc, portfolioId);

            // Requerimiento: si existe data para la fecha y portfolio, borrar y reinsertar
            await writeRepository.DeleteByClosingDateAndPortfolioAsync(closingDateUtc, portfolioId, cancellationToken);

            var readRows = 0;
            var insertedRows = 0;

            var batch = new List<ClosingSheet>(ReportsBulkProperties.EtlBatchSize);

            await foreach (var row in readRepository.ReadClosingAsync(closingDateUtc, portfolioId, cancellationToken))
            {
                if (row.FundId is null)
                {
                    return Result.Failure<EtlExecutionMetrics>(
                    new Error(
                    "ETL_CLOSING_NULL_FUND",
                    $"ClosingSheet no se puede generar. FundId es NULL. PortfolioId={row.PortfolioId}, ClosingDate={row.ClosingDate:yyyy-MM-dd}",
                    ErrorType.Failure));
                }

                readRows++;

                var createResult = ClosingSheet.Create(row);
                if (!createResult.IsSuccess || createResult.Value is null)
                {
                    return Result.Failure<EtlExecutionMetrics>(
                new Error("ETL_CLOSING_002", "Error creando fila ClosingSheet.", ErrorType.Failure));
                }

                batch.Add(createResult.Value);

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
                 "ETLClosing finalizó. ReadRows={ReadRows} InsertedRows={InsertedRows}",
                 readRows, insertedRows);

            return Result.Success(new EtlExecutionMetrics(readRows, insertedRows));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ETLClosing falló.");

            return Result.Failure<EtlExecutionMetrics>(
            new Error("ETL_CLOSING_001", $"ETLClosing falló: {exception.Message}", ErrorType.Failure));
        }
    }
}