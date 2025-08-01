using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestration;

public class ConfirmClosingOrchestrator(
    IDistributeTrustYieldsService trustYieldsDistribution,
    IValidateTrustYieldsDistributionService trustYieldsValidation,
    IPostClosingEventsOrchestation postClosingEventsOrchestation,
    ILogger<ConfirmClosingOrchestrator> logger)
    : IConfirmClosingOrchestrator
{
    public async Task<Result<ClosedResult>> ConfirmAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);

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

        // Paso 3: Disparar eventos de post - cierre
        // El evento de procesar las transacciones pendientes se encarga de activar el flujo transaccional
        await postClosingEventsOrchestation.ExecuteAsync(portfolioId, closingDate, ct);
        //if (postClosingEventsOrchestation.Errors.Any())
        //{
        //    logger.LogWarning("Errores en eventos post-cierre para portafolio {PortfolioId}: {Errors}", portfolioId, postClosingEventsOrchestation.Errors);
        //    return Result.Failure<ClosedResult>(new Error("002", "Errores en eventos post-cierre.", ErrorType.Validation));
        //}

        logger.LogInformation("Cierre confirmado exitosamente para portafolio {PortfolioId}", portfolioId);
        return Result.Success(new ClosedResult(portfolioId, closingDate));
    }
}