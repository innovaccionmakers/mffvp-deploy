namespace Common.SharedKernel.Domain.Finance;

/// <summary>
/// Contiene funciones financieras comunes para rentabilidad, tasas y valorizaciones.
/// </summary>
public static class FinancialMath
{
    /// <summary>
    /// Calcula rentabilidad compuesta anual (ej. rentabilidad diaria anualizada).
    /// </summary>
    public static decimal CalculateCompoundReturn(decimal baseValue, decimal newValue, int periodsPerYear)
    {
        if (baseValue <= 0) return 0;
        return (decimal)Math.Pow((double)(newValue / baseValue), periodsPerYear) - 1;
    }

    /// <summary>
    /// Calcula valor de unidad en función del valor anterior del portafolio más el rendimiento abonado.
    /// </summary>
    public static decimal CalculateUnitValue(decimal previousPortfolioValue, decimal yieldToCredit, decimal units)
    {
        if (units <= 0) return 0;
        return (previousPortfolioValue + yieldToCredit) / units;
    }

    // Agregar otras funciones, como TIR o tasa efectiva de ser necesarias
}
