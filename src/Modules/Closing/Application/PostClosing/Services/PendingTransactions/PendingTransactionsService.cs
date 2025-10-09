
using Closing.Application.Abstractions.External.Operations.ClientOperations;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Microsoft.Extensions.Logging;

namespace Closing.Application.PostClosing.Services.PendingTransactions;

/// <summary>
/// Orquesta el procesamiento de transacciones pendientes y, al completar,
/// activa el flujo transaccional (ClosingEnd) mediante ITimeControlService.
/// </summary>
public sealed class PendingTransactionsService : IPendingTransactionsService
{
    private readonly IProcessPendingTransactionsRemote processPendingTransactionsRemote;
    private readonly ITimeControlService timeControlService;
    private readonly ILogger<PendingTransactionsService> logger;

    public PendingTransactionsService(
        IProcessPendingTransactionsRemote processPendingTransactionsRemote,
        ITimeControlService timeControlService,
        ILogger<PendingTransactionsService> logger)
    {
        this.processPendingTransactionsRemote = processPendingTransactionsRemote;
        this.timeControlService = timeControlService;
        this.logger = logger;
    }

    public async Task HandleAsync(int portfolioId, DateTime processDateUtc, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var idempotencyKey = $"pendingtx:{portfolioId}:{processDateUtc:yyyyMMdd}";

        logger.LogInformation(
            "[{Class}] Iniciando procesamiento de transacciones pendientes Portafolio={PortfolioId} Fecha={Date} (IdemKey={Key})",
            nameof(PendingTransactionsService), portfolioId, processDateUtc, idempotencyKey);

        var request = new ProcessPendingTransactionsRemoteRequest(
            PortfolioId: portfolioId,
            ProcessDateUtc: processDateUtc,
            IdempotencyKey: idempotencyKey,
            ExecutionId: null 
        );

        var result = await processPendingTransactionsRemote.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogError(
                "[{Class}] Error  al procesar pendientes: {Code} {Description}",
                nameof(PendingTransactionsService), result.Error.Code, result.Error.Description);

            throw new InvalidOperationException($"ProcessPendingTransactionsRemote failed: {result.Error.Code} {result.Error.Description}");
        }

        var response = result.Value;

        var success = response.Succeeded && (response.Status == "Processed" || response.Status == "NothingToProcess");
        if (!success)
        {
            logger.LogError(
                "[{Class}] Falla en procesamiento de pendientes. Status={Status} Msg={Msg}",
                nameof(PendingTransactionsService), response.Status, response.Message);

            throw new InvalidOperationException($"ProcessPendingTransactions failed: {response.Status}. {response.Message}");
        }

        logger.LogInformation(
            "[{Class}] Pendientes procesadas. Applied={Applied} Skipped={Skipped}. Activando flujo transaccional…",
            nameof(PendingTransactionsService), response.AppliedCount, response.SkippedCount);

        await timeControlService.EndAsync(portfolioId, cancellationToken);

        logger.LogInformation(
            "[{Class}] Flujo transaccional activado para Portafolio={PortfolioId} Fecha={Date}",
            nameof(PendingTransactionsService), portfolioId, processDateUtc);
    }
}