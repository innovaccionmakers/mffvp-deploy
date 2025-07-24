using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution;
public class DistributeTrustYieldsService(
    ITrustYieldRepository trustYieldRepository,
    IYieldRepository yieldRepository,
    IPortfolioValuationRepository portfolioValuationRepository,
    ITimeControlService timeControlService,
    ILogger<DistributeTrustYieldsService> logger)
    : IDistributeTrustYieldsService
{
    private const decimal YieldRetentionRate = 0.10m;

    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        logger.LogInformation("Iniciando distribución de rendimientos para portafolio {PortfolioId}", portfolioId);

        var now = DateTime.UtcNow;

        // ⬅️ Se actualiza el paso de cierre usando el flujo estándar
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocation", now, ct);

        var yield = await yieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (yield is null)
        {
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }

        var portfolioValuation = await portfolioValuationRepository.GetValuationAsync(portfolioId, closingDate, ct);
        if (portfolioValuation is null)
        {
            return Result.Failure(new Error("002", "No existe valoración del portafolio para la fecha indicada.", ErrorType.Failure));
        }

        var trustYields = await trustYieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (!trustYields.Any())
        {
            return Result.Failure(new Error("003", "No existen registros en rendimientos_fideicomisos para esta fecha. Debe reprocesarse la réplica de datos.", ErrorType.Failure));
        }

        foreach (var trust in trustYields)
        {
            if (trust.Participation == 0m)
                continue;

            var yieldAmount = yield.YieldToCredit * trust.Participation;
            var income = yield.Income * trust.Participation;
            var expenses = yield.Expenses * trust.Participation;
            var commissions = yield.Commissions * trust.Participation;
            var cost = yield.Costs * trust.Participation;
            var closingBalance = trust.PreClosingBalance + yieldAmount;

            decimal units = 0m;
            if (trust.PreClosingBalance != trust.ClosingBalance)
                units = Math.Round(closingBalance / portfolioValuation.UnitValue, 16);

            var yieldRetention = Math.Round(yieldAmount * YieldRetentionRate, 16);

            trust.UpdateDetails(
                trust.TrustId,
                trust.PortfolioId,
                trust.ClosingDate,
                trust.Participation,
                units,
                yieldAmount,
                trust.PreClosingBalance,
                closingBalance,
                income,
                expenses,
                commissions,
                cost,
                trust.Capital,
                now,
                trust.ContingentRetention,
                yieldRetention
            );
        }

        await trustYieldRepository.SaveChangesAsync(ct);

        logger.LogInformation("Distribución de rendimientos finalizada para portafolio {PortfolioId}", portfolioId);
        return Result.Success();
    }
}
