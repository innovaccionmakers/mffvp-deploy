using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
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
    ILogger<ConfirmClosingOrchestrator> logger)
    : IConfirmClosingOrchestrator
{
    public async Task<Result<ConfirmClosingResult>> ConfirmAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Confirmando cierre para portafolio {PortfolioId}", portfolioId);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Paso 1: Distribuir rendimientos
            var distributionResult = await trustYieldsDistribution.RunAsync(portfolioId, closingDate, cancellationToken);
            if (distributionResult.IsFailure)
            {
                logger.LogWarning("Falló distribución de rendimientos para portafolio {PortfolioId}: {Error}", portfolioId, distributionResult.Error.Description);
                return Result.Failure<ConfirmClosingResult>(distributionResult.Error);
            }

            cancellationToken.ThrowIfCancellationRequested();
            // Paso 2: Validar rendimientos distribuidos
            var validationResult = await trustYieldsValidation.RunAsync(portfolioId, closingDate, cancellationToken);
            if (validationResult.IsFailure)
            {
                logger.LogWarning("Falló validación de distribución de rendimientos para portafolio {PortfolioId}: {Error}", portfolioId, validationResult.Error.Description);
                return Result.Failure<ConfirmClosingResult>(validationResult.Error);
            }

            cancellationToken.ThrowIfCancellationRequested();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var generalResult = new ConfirmClosingResult(portfolioId, closingDate);
            var warnings = warningCollector.GetAll();
            generalResult.HasWarnings = warnings.Any();
            generalResult.Warnings = warnings;

            logger.LogInformation("Cierre confirmado exitosamente para portafolio {PortfolioId}", portfolioId);

            cancellationToken.ThrowIfCancellationRequested();
            return Result.Success(generalResult);

        }
        catch (Exception ex)
        {
            await abortClosingService.AbortAsync(portfolioId, closingDate, cancellationToken);
            logger.LogInformation(
                ex,
                "Error inesperado en ConfirmClosingOrchestrator para Portafolio {PortfolioId}",
                portfolioId);

            return Result.Failure<ConfirmClosingResult>(
                new Error("001", "Error inesperado durante el cierre.", ErrorType.Failure));
        }

     
    }
}