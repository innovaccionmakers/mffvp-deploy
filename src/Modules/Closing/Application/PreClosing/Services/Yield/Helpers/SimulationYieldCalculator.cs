using Closing.Application.PreClosing.Services.Yield.Dto;
using Common.SharedKernel.Application.Constants;
using Common.SharedKernel.Application.Helpers.Finance;

namespace  Closing.Application.PreClosing.Services.Yield.Helpers;
public static class SimulationYieldCalculator
{
    public static SimulationValues Calculate(
        decimal yieldToCredit,
        decimal? previousPortfolioValue,
        decimal? previousUnitValue,
        decimal? previousUnits)
    {
        if (!previousPortfolioValue.HasValue || !previousUnitValue.HasValue || !previousUnits.HasValue)
            return new SimulationValues(null, null);

        if (previousUnits.Value <= 0 || previousUnitValue.Value <= 0)
            return new SimulationValues(null, null);

        var unitValue = PortfolioMath.CalculateUnitValue(
            previousPortfolioValue.Value, yieldToCredit, previousUnits.Value);

        var dailyProfitability = PortfolioMath.CalculateRoundedDailyProfitability(
            previousUnitValue.Value, unitValue, DecimalPrecision.SixteenDecimals);

        return new SimulationValues(unitValue, dailyProfitability);
    }


}