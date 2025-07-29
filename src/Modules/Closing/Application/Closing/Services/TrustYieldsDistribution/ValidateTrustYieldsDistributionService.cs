using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.TrustYields;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution;

public class ValidateTrustYieldsDistributionService(
    IYieldRepository yieldRepository,
    ITrustYieldRepository trustYieldRepository,
    IYieldDetailCreationService yieldDetailCreationService,
    ITimeControlService timeControlService,
    ILogger<ValidateTrustYieldsDistributionService> logger)
    : IValidateTrustYieldsDistributionService
{
    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        logger.LogInformation("Validando distribución de rendimientos para portafolio {PortfolioId}", portfolioId);

        var now = DateTime.UtcNow;

        // Publicar evento de paso
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", now, ct);

        var yield = await yieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (yield is null)
        {
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }

        var trustYields = await trustYieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (!trustYields.Any())
        {
            return Result.Failure(new Error("002", "No existen registros de rendimientos distribuidos para fideicomisos.", ErrorType.Failure));
        }

        var distributedTotal = trustYields.Sum(x => x.YieldAmount);
        var expectedTotal = yield.YieldToCredit;
        var difference = distributedTotal - expectedTotal;

        logger.LogInformation("Distribuido: {Distribuido}, Esperado: {Esperado}, Diferencia: {Diferencia}",
            distributedTotal, expectedTotal, difference);

        yield.UpdateDetails(
            portfolioId: yield.PortfolioId,
            income: yield.Income,
            expenses: yield.Expenses,
            commissions: yield.Commissions,
            costs: yield.Costs,
            yieldToCredit: yield.YieldToCredit,
            creditedYields: distributedTotal,
            closingDate: yield.ClosingDate,
            processDate: yield.ProcessDate,
            isClosed: yield.IsClosed
        );

        await yieldRepository.SaveChangesAsync(ct);

        if (difference != 0)
        {
            var nextClosingDate = closingDate.AddDays(1);

            var conceptText = difference > 0
                ? "Ajuste Rendimiento Ingreso"
                : "Ajuste Rendimiento Gasto";

            var conceptJson = JsonDocument.Parse($"\"{conceptText}\"");

            var yieldDetailResult = YieldDetail.Create(
                portfolioId: portfolioId,
                closingDate: nextClosingDate,
                source: "Concepto Automático",
                concept: conceptJson,
                income: difference,
                expenses: 0,
                commissions: 0,
                processDate: now,
                isClosed: true
            );

            if (yieldDetailResult.IsFailure)
                return Result.Failure(yieldDetailResult.Error);

            await yieldDetailCreationService.CreateYieldDetailsAsync(new[] { yieldDetailResult.Value }, ct);

            logger.LogInformation("Se generó ajuste de rendimiento para el portafolio {PortfolioId} en el día siguiente", portfolioId);
        }
        else
        {
            logger.LogInformation("No se detectaron diferencias en rendimientos distribuidos para portafolio {PortfolioId}", portfolioId);
        }

        return Result.Success();
    }
}