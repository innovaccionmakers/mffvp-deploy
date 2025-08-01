using Closing.Application.Closing.Services.Orchestation.Constants;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution;
public class DistributeTrustYieldsService(
    ITrustYieldRepository trustYieldRepository,
    IYieldRepository yieldRepository,
    IPortfolioValuationRepository portfolioValuationRepository,
    ITimeControlService timeControlService,
    IConfigurationParameterRepository configurationParameterRepository,
    ILogger<DistributeTrustYieldsService> logger)
    : IDistributeTrustYieldsService
{
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

        var param = await configurationParameterRepository
             .GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldRetentionPercentage, ct);

        if (param is null)
        {
            return Result.Failure(new Error("004", "No se encuentra configurado el parametro de retención de rendimientos.", ErrorType.Failure));
        }

        var yieldRetentionRate = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "valor", true);

        if (yieldRetentionRate <= 0)
        {
            return Result.Failure(new Error("005", "El parametro de retención de rendimientos no es un valor mayor a 0", ErrorType.Failure));
        }


        foreach (var trust in trustYields)
        {
            var participation = 0m;
            var prevTrustYield = await trustYieldRepository.GetByTrustAndDateAsync(trust.TrustId, closingDate.AddDays(-1), ct);
            var prevPortfolioValuation = await portfolioValuationRepository.GetValuationAsync(portfolioId, closingDate.AddDays(-1), ct);
            if (prevTrustYield is not null && prevPortfolioValuation is not null)
                 participation = TrustMath.CalculateTrustParticipation(prevTrustYield.ClosingBalance,prevPortfolioValuation.Amount, DecimalPrecision.SixteenDecimals);
            else
                participation = TrustMath.CalculateTrustParticipation(trust.PreClosingBalance, portfolioValuation.Amount, DecimalPrecision.SixteenDecimals);
            var yieldAmount = TrustMath.ApplyParticipation(yield.YieldToCredit, participation, DecimalPrecision.SixteenDecimals);
            var income = TrustMath.ApplyParticipation(yield.Income, participation, DecimalPrecision.SixteenDecimals);
            var expenses = TrustMath.ApplyParticipation(yield.Expenses, participation, DecimalPrecision.SixteenDecimals);
            var commissions = TrustMath.ApplyParticipation(yield.Commissions, participation, DecimalPrecision.SixteenDecimals);
            var cost = TrustMath.ApplyParticipation(yield.Costs, participation, DecimalPrecision.SixteenDecimals);
            var closingBalance = trust.PreClosingBalance + yieldAmount;

            decimal units = 0m;
            if (trust.PreClosingBalance != closingBalance)
                units = Math.Round(closingBalance / portfolioValuation.UnitValue, DecimalPrecision.SixteenDecimals);
            if (prevTrustYield is not null && prevPortfolioValuation is not null)
                units = Math.Round(prevTrustYield.Units, DecimalPrecision.SixteenDecimals);
            else
                units = Math.Round(closingBalance / portfolioValuation.UnitValue, DecimalPrecision.SixteenDecimals); // para el primer día de cierre

            var yieldRetention = TrustMath.CalculateYieldRetention(yieldAmount, yieldRetentionRate, DecimalPrecision.SixteenDecimals);

            trust.UpdateDetails(
                trust.TrustId,
                trust.PortfolioId,
                trust.ClosingDate,
                participation,
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
