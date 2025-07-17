using Common.SharedKernel.Application.Helpers.Finance;

namespace Closing.Domain.PreClosing;

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

        var unitValue = FinancialMath.CalculateUnitValue(
            previousPortfolioValue.Value, yieldToCredit, previousUnits.Value);

        var dailyProfitability = FinancialMath.CalculateCompoundReturn(
            previousUnitValue.Value, unitValue, 365);

        return new SimulationValues(unitValue, dailyProfitability);
    }


}