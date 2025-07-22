
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.TimeControl;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestation;

public class CancelClosingOrchestrator(
    //ITransactionApplierService transactionApplier,
    ITimeControlService timeControl,
    ILogger<CancelClosingOrchestrator> logger)
    : ICancelClosingOrchestrator
{
    public async Task<Result<ClosedResult>> CancelAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        logger.LogInformation("Cancelando cierre para portafolio {PortfolioId}", portfolioId);

        //var applyResult = await transactionApplier.ExecuteAsync(portfolioId, closingDate, ct);
        //if (applyResult.IsFailure)
        //    return Result.Failure<ClosedResult>(applyResult.Error);

        await timeControl.EndAsync(portfolioId, ct);

        logger.LogInformation("Cierre cancelado correctamente para portafolio {PortfolioId}", portfolioId);
        return Result.Success(new ClosedResult(portfolioId, closingDate));
    }
}