using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.TimeControl;
using Closing.Application.Closing.Services.TrustYieldsDistribution;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestration;

public class ConfirmClosingOrchestrator(
    IDistributeTrustYieldsService yieldDistributionService,
    //IYieldValidationService yieldValidationService,
    //ITransactionApplierService transactionApplier,
    ITimeControlService timeControl,
    ILogger<ConfirmClosingOrchestrator> logger)
    : IConfirmClosingOrchestrator
{
    public async Task<Result<ClosedResult>> ConfirmAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        logger.LogInformation("Confirmando cierre para portafolio {PortfolioId}", portfolioId);

        var distributionResult = await yieldDistributionService.RunAsync(portfolioId, closingDate, ct);
        if (distributionResult.IsFailure)
            return Result.Failure<ClosedResult>(distributionResult.Error);

        //var validationResult = await yieldValidationService.ExecuteAsync(portfolioId, closingDate, ct);
        //if (validationResult.IsFailure)
        //    return Result.Failure<ClosedResult>(validationResult.Error);

        //var applyResult = await transactionApplier.ExecuteAsync(portfolioId, closingDate, ct);
        //if (applyResult.IsFailure)
        //    return Result.Failure<ClosedResult>(applyResult.Error);

        await timeControl.EndAsync(portfolioId, ct);

        logger.LogInformation("Cierre confirmado exitosamente para portafolio {PortfolioId}", portfolioId);
        return Result.Success(new ClosedResult(portfolioId, closingDate));
    }
}
