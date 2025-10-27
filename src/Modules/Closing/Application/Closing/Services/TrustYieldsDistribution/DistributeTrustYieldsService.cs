
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
    ITrustYieldBulkRepository trustYieldBulkRepository, 
    IYieldRepository yieldRepository,
    IPortfolioValuationRepository portfolioValuationRepository,
    ITimeControlService timeControlService,
    IConfigurationParameterRepository configurationParameterRepository,
    ILogger<DistributeTrustYieldsService> logger)
    : IDistributeTrustYieldsService
{
    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["Service"] = "DistributeTrustYieldsService",
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate.Date
        });
        const string svc = "[DistributeTrustYieldsService]";

        static DateTime ToUtcMidnight(DateTime dt)
            => (dt.Kind == DateTimeKind.Utc ? dt.Date : DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc));
        var nowUtc = DateTime.UtcNow;
        var closingDateUtc = ToUtcMidnight(closingDate);

        // Paso de control de tiempo
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocation", nowUtc, cancellationToken);

        // Rendmientos a distribuir
        var yield = await yieldRepository.GetReadOnlyToDistributeByPortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);
        if (yield is null)
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));

        var portfolioValuation = await portfolioValuationRepository.GetReadOnlyToDistributePortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);
        if (portfolioValuation is null)
            return Result.Failure(new Error("002", "No existe valoración del portafolio para la fecha indicada.", ErrorType.Failure));

        var trustReadOnly = await trustYieldRepository.GetCalcInputsByPortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);
        if (trustReadOnly.Count == 0)
            return Result.Failure(new Error("003", "No existen registros en rendimientos_fideicomisos para esta fecha. Debe reprocesarse la réplica de datos.", ErrorType.Failure));

        logger.LogInformation("{Svc} Se encontraron {Count} registros de rendimientos_fideicomisos para distribuir", svc, trustReadOnly.Count);

        // Parámetro de retención
        var param = await configurationParameterRepository.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldRetentionPercentage, cancellationToken);
        if (param is null)
            return Result.Failure(new Error("004", "No se encuentra configurado el parametro de retención de rendimientos.", ErrorType.Failure));

        var yieldRetentionRate = JsonDecimalHelper.ExtractDecimal(param.Metadata, "valor", true);
        if (yieldRetentionRate <= 0)
            return Result.Failure(new Error("005", "El parametro de retención de rendimientos no es un valor mayor a 0", ErrorType.Failure));

        // Datos del día previo
        var previousDateUtc = ToUtcMidnight(closingDateUtc.AddDays(-1));
        var prevPortfolioValuation = await portfolioValuationRepository.GetReadOnlyToDistributePortfolioAndDateAsync(portfolioId, previousDateUtc, cancellationToken);
        if (prevPortfolioValuation is null)
            logger.LogWarning("{Svc} No se encontró valuación previa del portafolio para la fecha {PreviousClosingDate}", svc, previousDateUtc);

        // Precálculos
        var yToCredit = MoneyHelper.Round2(yield.YieldToCredit);
        var yIncome = MoneyHelper.Round2(yield.Income);
        var yExpenses = MoneyHelper.Round2(yield.Expenses);
        var yFees = MoneyHelper.Round2(yield.Commissions);
        var yCosts = MoneyHelper.Round2(yield.Costs);

        var hasPrevPV = prevPortfolioValuation is { Amount: > 0m };
        var hasUnit = portfolioValuation.UnitValue > 0m;
        var invUnit = hasUnit ? (1m / portfolioValuation.UnitValue) : 0m;

        var rows = new List<TrustYieldUpdateRow>(capacity: Math.Min(trustReadOnly.Count, 4096));
        var po = new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = cancellationToken };

        Parallel.ForEach(
            source: trustReadOnly,
            parallelOptions: po,
            localInit: () => new List<TrustYieldUpdateRow>(512),
            body: (trust, state, local) =>
            {
                // Participación basada en saldo_pre_cierre / monto portafolio previo

                decimal participation = 0m;
                if (hasPrevPV && trust.prevDayClosingBalance != 0m)
                    participation = TrustMath.CalculateTrustParticipation(trust.prevDayClosingBalance, prevPortfolioValuation!.Amount, DecimalPrecision.SixteenDecimals);

                // Prorrateos
                var yieldAmount16 = TrustMath.ApplyParticipation(yToCredit, participation, DecimalPrecision.SixteenDecimals);
                var income16 = TrustMath.ApplyParticipation(yIncome, participation, DecimalPrecision.SixteenDecimals);
                var expenses16 = TrustMath.ApplyParticipation(yExpenses, participation, DecimalPrecision.SixteenDecimals);
                var commissions16 = TrustMath.ApplyParticipation(yFees, participation, DecimalPrecision.SixteenDecimals);
                var cost16 = TrustMath.ApplyParticipation(yCosts, participation, DecimalPrecision.SixteenDecimals);

                var yieldAmtR2 = MoneyHelper.Round2(yieldAmount16);
                var incomeR2 = MoneyHelper.Round2(income16);
                var expensesR2 = MoneyHelper.Round2(expenses16);
                var commissionsR2 = MoneyHelper.Round2(commissions16);
                var costR2 = MoneyHelper.Round2(cost16);

                var pre = MoneyHelper.Round2(trust.PreClosingBalance);
                var closingBalance = MoneyHelper.Round2(pre + yieldAmtR2);

                decimal units;
                var balanceChanged = pre != closingBalance;
                if (!balanceChanged)
                    units = Math.Round(trust.Units, DecimalPrecision.SixteenDecimals);
                else
                    units = hasUnit ? Math.Round((trust.PreClosingBalance + yieldAmount16) * invUnit, DecimalPrecision.SixteenDecimals) : 0m;

                var yieldRetentionR2 = (yToCredit > 0m && yieldAmtR2 > 0m)
                    ? MoneyHelper.Round2(TrustMath.CalculateYieldRetention(yieldAmtR2, yieldRetentionRate, DecimalPrecision.TwoDecimals))
                    : 0m;

                    local.Add(new TrustYieldUpdateRow(
                        TrustId: trust.TrustId,
                        PortfolioId: trust.PortfolioId,
                        ClosingDateUtc: closingDateUtc,
                        Participation: Math.Round(participation, DecimalPrecision.SixteenDecimals),
                        Units: units,
                        YieldAmount: yieldAmtR2,
                        Income: incomeR2,
                        Expenses: expensesR2,
                        Commissions: commissionsR2,
                        Cost: costR2,
                        ClosingBalance: closingBalance,
                        YieldRetention: yieldRetentionR2,
                        ProcessDateUtc: nowUtc
                    ));

                return local;
            },
            localFinally: local =>
            {
                if (local.Count == 0) return;
                lock (rows) rows.AddRange(local); 
            }
        );

        if (rows.Count == 0)
        {
            logger.LogInformation("{Svc} No hay cambios para aplicar (delta=0).", svc);
            return Result.Success();
        }

        await trustYieldBulkRepository.BulkUpdateAsync(rows, cancellationToken);

        return Result.Success();
    }
}
