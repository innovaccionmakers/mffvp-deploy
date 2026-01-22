using Common.SharedKernel.Application.Constants.Reports;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Reports.Application.LoadingInfo.Contracts;
using Reports.Application.LoadingInfo.Models;
using Reports.Domain.LoadingInfo.Balances;

namespace Reports.Application.LoadingInfo.Features.Balances;

public sealed class EtlBalancesService : IBalancesLoader
{
    private readonly IBalanceSheetReadRepository readRepository;
    private readonly IBalanceSheetWriteRepository writeRepository;
    private readonly ILogger<EtlBalancesService> logger;

    public EtlBalancesService(
IBalanceSheetReadRepository readRepository,
        IBalanceSheetWriteRepository writeRepository,
        ILogger<EtlBalancesService> logger)
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
            logger.LogInformation(
             "ETLSaldos iniciado. ClosingDate={ClosingDate} PortfolioId={PortfolioId}",
            closingDateUtc, portfolioId);

            // Regla: si ya existe data para la fecha y portafolio, borrar y reinsertar
            await writeRepository.DeleteByClosingDateAndPortfolioAsync(closingDateUtc, portfolioId, cancellationToken);

            var readRows = 0;
            var insertedRows = 0;

            var batch = new List<BalanceSheet>(ReportsBulkProperties.EtlBatchSize);

            await foreach (var row in readRepository.ReadBalancesAsync(closingDateUtc, portfolioId, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                readRows++;

                var createResult = BalanceSheet.Create(row);
                if (!createResult.IsSuccess || createResult.Value is null)
                {
                    return Result.Failure<EtlExecutionMetrics>(createResult.Error);
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
             "ETLSaldos finalizó. ReadRows={ReadRows} InsertedRows={InsertedRows}",
             readRows, insertedRows);

            return Result.Success(new EtlExecutionMetrics(readRows, insertedRows));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ETLSaldos falló.");

            return Result.Failure<EtlExecutionMetrics>(
            new Error("ETL_SALDOS_001", $"ETLSaldos falló: {exception.Message}", ErrorType.Failure));
        }
    }
}