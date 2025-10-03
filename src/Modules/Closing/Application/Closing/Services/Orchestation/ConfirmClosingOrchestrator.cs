using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.Telemetry;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.Closing.Services.Warnings;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestration;

public class ConfirmClosingOrchestrator(
    IDistributeTrustYieldsService trustYieldsDistribution,
    IValidateTrustYieldsDistributionService trustYieldsValidation,
     IWarningCollector warningCollector,
    IAbortClosingService abortClosingService,
    IUnitOfWork unitOfWork,
    IClosingStepTimer stepTimer,
    ILogger<ConfirmClosingOrchestrator> logger)
    : IConfirmClosingOrchestrator
{
    public async Task<Result<ConfirmClosingResult>> ConfirmAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Paso 1: Distribuir rendimientos
            using (stepTimer.Track("ConfirmClosingOrchestrator.DistributeTrustYields", portfolioId, closingDate))
            {
                var distributionResult = await trustYieldsDistribution.RunAsync(portfolioId, closingDate, cancellationToken);
                if (distributionResult.IsFailure)
                {
                    logger.LogWarning("Falló distribución de rendimientos para portafolio {PortfolioId}: {Error}", portfolioId, distributionResult.Error.Description);
                    return Result.Failure<ConfirmClosingResult>(distributionResult.Error);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            // Paso 2: Validar rendimientos distribuidos
            using (stepTimer.Track("ConfirmClosingOrchestrator.ValidateTrustYields", portfolioId, closingDate))
            {
                var validationResult = await trustYieldsValidation.RunAsync(portfolioId, closingDate, cancellationToken);
                if (validationResult.IsFailure)
                {
                    logger.LogWarning("Falló validación de distribución de rendimientos para portafolio {PortfolioId}: {Error}", portfolioId, validationResult.Error.Description);
                    return Result.Failure<ConfirmClosingResult>(validationResult.Error);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            //Paso 3: Guardar todo en base de datos
            using (stepTimer.Track("ConfirmClosingOrchestrator.UnitOfWork.SaveChanges", portfolioId, closingDate))
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            var generalResult = new ConfirmClosingResult(portfolioId, closingDate);
            var warnings = warningCollector.GetAll();
            generalResult.HasWarnings = warnings.Any();
            generalResult.Warnings = warnings;

            cancellationToken.ThrowIfCancellationRequested();
            return Result.Success(generalResult);

        }
        catch (Exception ex)
        {
            await abortClosingService.AbortAsync(portfolioId, closingDate, cancellationToken);
            logger.LogError(
                ex,
                "Error inesperado en ConfirmClosingOrchestrator para Portafolio {PortfolioId}",
                portfolioId);

            return Result.Failure<ConfirmClosingResult>(
                new Error("001", "Error inesperado durante el cierre.", ErrorType.Failure));
        }

     
    }
}