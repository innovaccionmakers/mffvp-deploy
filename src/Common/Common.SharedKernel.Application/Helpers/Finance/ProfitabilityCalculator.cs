using System;

namespace Common.SharedKernel.Application.Helpers.Finance;

/// <summary>
/// Default implementation for profitability calculations.
/// </summary>
public sealed class ProfitabilityCalculator : IProfitabilityCalculator
{
    public decimal AnnualizedPercentage(decimal finalValue, decimal initialValue, int days)
    {
        if (initialValue <= 0 || days <= 0)
        {
            return 0m;
        }

        var ratio = (double)(finalValue / initialValue);
        var exponent = 365d / days;
        var result = (Math.Pow(ratio, exponent) - 1d) * 100d;
        return (decimal)result;
    }
}

