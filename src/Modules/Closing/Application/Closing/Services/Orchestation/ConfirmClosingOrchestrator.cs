using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestration;

public class ConfirmClosingOrchestrator(
    IDistributeTrustYieldsService trustYieldsDistribution,
    IValidateTrustYieldsDistributionService trustYieldsValidation,
    ITimeControlService timeControl,
    ILogger<ConfirmClosingOrchestrator> logger)
    : IConfirmClosingOrchestrator
{
    public async Task<Result<ClosedResult>> ConfirmAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        logger.LogInformation("Confirmando cierre para portafolio {PortfolioId}", portfolioId);

        // Paso 1: Distribuir rendimientos
        var distributionResult = await trustYieldsDistribution.RunAsync(portfolioId, closingDate, ct);
        if (distributionResult.IsFailure)
        {
            logger.LogWarning("Falló distribución de rendimientos para portafolio {PortfolioId}: {Error}", portfolioId, distributionResult.Error.Description);
            return Result.Failure<ClosedResult>(distributionResult.Error);
        }

        // Paso 2: Validar rendimientos distribuidos
        var validationResult = await trustYieldsValidation.RunAsync(portfolioId, closingDate, ct);
        if (validationResult.IsFailure)
        {
            logger.LogWarning("Falló validación de distribución de rendimientos para portafolio {PortfolioId}: {Error}", portfolioId, validationResult.Error.Description);
            return Result.Failure<ClosedResult>(validationResult.Error);
        }

        // Paso 3: Finalizar flujo
        await timeControl.EndAsync(portfolioId, ct);

        logger.LogInformation("Cierre confirmado exitosamente para portafolio {PortfolioId}", portfolioId);
        return Result.Success(new ClosedResult(portfolioId, closingDate));
    }
}