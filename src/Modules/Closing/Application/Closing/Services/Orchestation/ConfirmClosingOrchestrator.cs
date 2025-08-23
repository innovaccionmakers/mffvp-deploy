using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Closing.Application.Closing.Services.Orchestration;

public class ConfirmClosingOrchestrator(
    IDistributeTrustYieldsService trustYieldsDistribution,
    IValidateTrustYieldsDistributionService trustYieldsValidation,

    IAbortClosingService abortClosingService,
    IUnitOfWork unitOfWork,
    ILogger<ConfirmClosingOrchestrator> logger)
    : IConfirmClosingOrchestrator
{
    public async Task<Result<ClosedResult>> ConfirmAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);

        logger.LogInformation("Confirmando cierre para portafolio {PortfolioId}", portfolioId);
        try
        {
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

            await unitOfWork.SaveChangesAsync(ct);
           


            logger.LogInformation("Cierre confirmado exitosamente para portafolio {PortfolioId}", portfolioId);

            return Result.Success(new ClosedResult(portfolioId, closingDate));

        }
        catch (Exception ex)
        {
            await abortClosingService.AbortAsync(portfolioId, closingDate, ct);
            logger.LogError(
                ex,
                "Error inesperado en ConfirmClosingOrchestrator para Portafolio {PortfolioId}",
                portfolioId);

            return Result.Failure<ClosedResult>(
                new Error("001", "Error inesperado durante el cierre.", ErrorType.Failure));
        }

     
    }
}