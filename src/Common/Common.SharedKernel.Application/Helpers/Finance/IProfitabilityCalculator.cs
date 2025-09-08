namespace Common.SharedKernel.Application.Helpers.Finance;

/// <summary>
/// Calculates annualized profitability percentages based on values and periods.
/// </summary>
public interface IProfitabilityCalculator
{
    /// <summary>
    /// Computes the annualized percentage return given a final and initial value over a number of days.
    /// Returns 0 when the initial value is less than or equal to zero.
    /// Formula: ((final/initial)^(365/days) - 1) * 100
    /// </summary>
    decimal AnnualizedPercentage(decimal finalValue, decimal initialValue, int days);
}

