using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Common.SharedKernel.Application.Constants;
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Helpers.Money;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
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
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["Service"] = "DistributeTrustYieldsService",
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate.Date
        });
        const string svc = "[DistributeTrustYieldsService]";

        logger.LogInformation("{Svc} Iniciando distribución de rendimientos para portafolio {PortfolioId}", svc, portfolioId);

        var now = DateTime.UtcNow;

        // ⬅️ Se actualiza el paso de cierre - camino feliz
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocation", now, ct);
        logger.LogInformation("{Svc} Paso de control de tiempo actualizado: NowUtc={NowUtc}", svc, now);

        var yield = await yieldRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (yield is null)
        {
            logger.LogWarning("{Svc} No se encontró información de rendimientos para la fecha {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Yield del día: Income={Income}, Expenses={Expenses}, Commissions={Commissions}, YieldToCredit={YieldToCredit}, Costs={Costs}",
            svc, yield.Income, yield.Expenses, yield.Commissions, yield.YieldToCredit, yield.Costs);

        var portfolioValuation = await portfolioValuationRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (portfolioValuation is null)
        {
            logger.LogWarning("{Svc} No existe valoración del portafolio para la fecha {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("002", "No existe valoración del portafolio para la fecha indicada.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Valuación del día: Amount={Amount}, Units={Units}, UnitValue={UnitValue}",
            svc, portfolioValuation.Amount, portfolioValuation.Units, portfolioValuation.UnitValue);

        var trustYields = await trustYieldRepository.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (!trustYields.Any())
        {
            logger.LogWarning("{Svc} No existen registros en rendimientos_fideicomisos para esta fecha", svc);
            return Result.Failure(new Error("003", "No existen registros en rendimientos_fideicomisos para esta fecha. Debe reprocesarse la réplica de datos.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Se encontraron {Count} registros de rendimientos_fideicomisos para distribuir", svc, trustYields.Count);

        var param = await configurationParameterRepository
             .GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldRetentionPercentage, ct);

        if (param is null)
        {
            logger.LogWarning("{Svc} Parámetro de retención de rendimientos no configurado", svc);
            return Result.Failure(new Error("004", "No se encuentra configurado el parametro de retención de rendimientos.", ErrorType.Failure));
        }

        var yieldRetentionRate = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "valor", true);
        logger.LogInformation("{Svc} Tasa de retención de rendimientos obtenida: {Rate}", svc, yieldRetentionRate);

        if (yieldRetentionRate <= 0)
        {
            logger.LogWarning("{Svc} El parámetro de retención de rendimientos no es mayor a 0: {Rate}", svc, yieldRetentionRate);
            return Result.Failure(new Error("005", "El parametro de retención de rendimientos no es un valor mayor a 0", ErrorType.Failure));
        }

        foreach (var trust in trustYields)
        {
            logger.LogInformation("{Svc} Procesando TrustId={TrustId}", svc, trust.TrustId);

            var participation = 0m;
            var prevTrustYield = await trustYieldRepository.GetReadOnlyByTrustAndDateAsync(trust.TrustId, closingDate.AddDays(-1), ct);
            var prevPortfolioValuation = await portfolioValuationRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate.AddDays(-1), ct);

            logger.LogInformation("{Svc} Datos previos: PrevTrustYieldExists={PrevTY}, PrevPortfolioValuationExists={PrevPV}",
                svc, prevTrustYield is not null, prevPortfolioValuation is not null);

            if (prevTrustYield is not null && prevPortfolioValuation is not null) {
                logger.LogInformation(
                   "{Svc} Valores previos: PrevTrustBalance={PrevTrustBalance}, PrevPortfolioAmount={PrevPortfolioAmount}",
                   svc,
                   prevTrustYield.ClosingBalance,
                   prevPortfolioValuation.Amount);
                participation = TrustMath.CalculateTrustParticipation(prevTrustYield.ClosingBalance, prevPortfolioValuation.Amount, DecimalPrecision.SixteenDecimals);
             }

            logger.LogInformation("{Svc} Participación calculada: {Participation}", svc, participation);

            var yieldAmount = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.YieldToCredit), participation, DecimalPrecision.SixteenDecimals);
            var income = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Income), participation, DecimalPrecision.SixteenDecimals);
            var expenses = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Expenses), participation, DecimalPrecision.SixteenDecimals);
            var commissions = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Commissions), participation, DecimalPrecision.SixteenDecimals);
            var cost = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Costs), participation, DecimalPrecision.SixteenDecimals);
            var closingBalance = MoneyHelper.Round2(trust.PreClosingBalance) + MoneyHelper.Round2(yieldAmount);

            logger.LogInformation("{Svc} Montos por participación: YieldToCredit={YieldAmount}, Income={Income}, Expenses={Expenses}, Commissions={Commissions}, Cost={Cost}",
                svc, yieldAmount, income, expenses, commissions, cost);
            logger.LogInformation("{Svc} Saldos: PreClosingBalance={Pre}, ClosingBalance={Close}", svc, trust.PreClosingBalance, closingBalance);

            decimal units = 0m;

            var balanceChanged = MoneyHelper.Round2(trust.PreClosingBalance) != MoneyHelper.Round2(closingBalance);

            if (!balanceChanged && prevTrustYield is not null)
            {
                units = Math.Round(prevTrustYield.Units, DecimalPrecision.SixteenDecimals);
            }
            else
            {
                units = Math.Round((trust.PreClosingBalance +  yieldAmount)
                                   / portfolioValuation.UnitValue, DecimalPrecision.SixteenDecimals);
            }


            logger.LogInformation("{Svc} Units final: {Units} (UnitValue={UnitValue}, PreclosingBalance={PreClosedBalance}," +
                " YieldAmount={YieldAmount}, ClosingBalance={ClosingBalance})", svc, units, portfolioValuation.UnitValue, trust.PreClosingBalance, yieldAmount, closingBalance);

            var yieldRetention = 0m;
            if ((yield.YieldToCredit > 0))
            {
                yieldRetention = TrustMath.CalculateYieldRetention(MoneyHelper.Round2(yieldAmount), yieldRetentionRate, DecimalPrecision.TwoDecimals);
                logger.LogInformation("{Svc} Retención de rendimientos calculada: {YieldRetention} con tasa {Rate}", svc, yieldRetention, yieldRetentionRate);
            }
            logger.LogInformation("{Svc} UpdateDetails => TrustId={TrustId}, Participation={Participation}, Units={Units}, YieldAmount={YieldAmount}, Income={Income}, Expenses={Expenses}, Commissions={Commissions}, Cost={Cost}, ClosingBalance={ClosingBalance}, Capital={Capital}, ContingentRetention={ContingentRetention}, YieldRetention={YieldRetention}",
                svc, trust.TrustId, participation, units, yieldAmount, income, expenses, commissions, cost, closingBalance, trust.Capital, trust.ContingentRetention, yieldRetention);

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
        logger.LogInformation("{Svc} Cambios persistidos en trust_yields.", svc);

        logger.LogInformation("Distribución de rendimientos finalizada para portafolio {PortfolioId}", portfolioId);
        logger.LogInformation("{Svc} Proceso finalizado correctamente.", svc);
        return Result.Success();
    }
}
