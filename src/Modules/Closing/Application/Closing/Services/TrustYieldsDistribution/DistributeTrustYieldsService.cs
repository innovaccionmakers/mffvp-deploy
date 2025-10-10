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

        var yield = await yieldRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (yield is null)
        {
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }

        var portfolioValuation = await portfolioValuationRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (portfolioValuation is null)
        {
            return Result.Failure(new Error("002", "No existe valoración del portafolio para la fecha indicada.", ErrorType.Failure));
        }

        var trustYields = await trustYieldRepository.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (!trustYields.Any())
        {
            return Result.Failure(new Error("003", "No existen registros en rendimientos_fideicomisos para esta fecha. Debe reprocesarse la réplica de datos.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Se encontraron {Count} registros de rendimientos_fideicomisos para distribuir", svc, trustYields.Count);

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

        var previousClosingDate = closingDate.AddDays(-1);
        var previousPortfolioValuation = await portfolioValuationRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, previousClosingDate, ct);

        if (previousPortfolioValuation is null)
        {
            logger.LogError("{Svc} No se encontró valuación previa del portafolio para la fecha {PreviousClosingDate}", svc, previousClosingDate);
        }

        var previousTrustYields = await trustYieldRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, previousClosingDate, ct);
        var previousTrustYieldByTrustId = new Dictionary<long, TrustYield>();

        foreach (var previousTrustYield in previousTrustYields)
        {
            previousTrustYieldByTrustId[previousTrustYield.TrustId] = previousTrustYield;
        }

        logger.LogInformation("{Svc} Rendimientos previos precargados para {Count} fideicomisos correspondientes al {PreviousClosingDate}",
            svc,
            previousTrustYieldByTrustId.Count,
            previousClosingDate);

        // ⬇️ OPTIMIZACIÓN: Cargar datos previos en batch (reduce de ~30,000 queries a solo 2)
        logger.LogInformation("{Svc} Cargando datos previos en batch para optimizar performance...", svc);
        var previousDate = closingDate.AddDays(-1);

        // Obtener prevPortfolioValuation una sola vez
        var prevPortfolioValuation = await portfolioValuationRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, previousDate, ct);

        // Obtener todos los prevTrustYields en una sola query (batch)
        var trustIds = trustYields.Select(t => t.TrustId).ToList();
        var prevTrustYieldsDict = await trustYieldRepository.GetReadOnlyByTrustIdsAndDateAsync(trustIds, previousDate, ct);

        logger.LogInformation("{Svc} Datos previos cargados: PrevPortfolioValuationExists={PrevPV}, PrevTrustYieldsCount={PrevTYCount}",
            svc, prevPortfolioValuation is not null, prevTrustYieldsDict.Count);

        // ⬇️ OPTIMIZACIÓN: Procesamiento paralelo con grado de paralelismo controlado
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 8, // Límite para no saturar recursos
            CancellationToken = ct
        };

        logger.LogInformation("{Svc} Iniciando procesamiento paralelo de {Count} trusts con MaxDegreeOfParallelism={MaxDegree}...",
            svc, trustYields.Count, parallelOptions.MaxDegreeOfParallelism);

        await Parallel.ForEachAsync(trustYields, parallelOptions, async (trust, token) =>
        {

            var participation = 0m;
            prevTrustYieldsDict.TryGetValue(trust.TrustId, out var prevTrustYield);;

            if (prevTrustYield is not null && prevPortfolioValuation is not null)
            {
                participation = TrustMath.CalculateTrustParticipation(prevTrustYield.ClosingBalance, prevPortfolioValuation.Amount, DecimalPrecision.SixteenDecimals);
            }


            var yieldAmount = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.YieldToCredit), participation, DecimalPrecision.SixteenDecimals);
            var income = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Income), participation, DecimalPrecision.SixteenDecimals);
            var expenses = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Expenses), participation, DecimalPrecision.SixteenDecimals);
            var commissions = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Commissions), participation, DecimalPrecision.SixteenDecimals);
            var cost = TrustMath.ApplyParticipation(MoneyHelper.Round2(yield.Costs), participation, DecimalPrecision.SixteenDecimals);
            var closingBalance = MoneyHelper.Round2(trust.PreClosingBalance) + MoneyHelper.Round2(yieldAmount);

            decimal units = 0m;

            var balanceChanged = MoneyHelper.Round2(trust.PreClosingBalance) != MoneyHelper.Round2(closingBalance);

            if (!balanceChanged && prevTrustYield is not null)
            {
                units = Math.Round(prevTrustYield.Units, DecimalPrecision.SixteenDecimals);
            }
            else
            {
                units = Math.Round((trust.PreClosingBalance + yieldAmount)
                                   / portfolioValuation.UnitValue, DecimalPrecision.SixteenDecimals);
            }

            var yieldRetention = 0m;
            if ((yield.YieldToCredit > 0))
            {
                yieldRetention = TrustMath.CalculateYieldRetention(MoneyHelper.Round2(yieldAmount), yieldRetentionRate, DecimalPrecision.TwoDecimals);
            }

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

            await Task.CompletedTask;
        });

        await trustYieldRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
