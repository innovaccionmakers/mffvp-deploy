namespace Common.SharedKernel.Application.Helpers.Finance;

public static class TrustMath
{
    /// <summary>
    /// Calcula la participación del fideicomiso respecto al valor del portafolio del día anterior.
    /// </summary>
    public static decimal CalculateTrustParticipation(decimal previousClosingBalance, decimal previousPortfolioAmount, int precision)
    {
        if (previousPortfolioAmount <= 0) return 0;
        return Math.Round(previousClosingBalance / previousPortfolioAmount, precision);
    }

    /// <summary>
    /// Aplica el porcentaje de participación a un valor dado.
    /// </summary>
    public static decimal ApplyParticipation(decimal value, decimal participation, int precision)
    {
        return Math.Round(value * participation, precision);
    }

    /// <summary>
    /// Calcula el nuevo saldo de cierre del fideicomiso.
    /// </summary>
    public static decimal CalculateClosingBalance(decimal preClosingBalance, decimal yieldAmount)
    {
        return preClosingBalance + yieldAmount;
    }

    /// <summary>
    /// Calcula el número de unidades del fideicomiso a partir del saldo final y valor de unidad.
    /// </summary>
    public static decimal CalculateTrustUnits(decimal closingBalance, decimal unitValue, int precision)
    {
        if (unitValue <= 0) return 0;
        return Math.Round(closingBalance / unitValue, precision);
    }

    /// <summary>
    /// Calcula la retención sobre el rendimiento del fideicomiso.
    /// </summary>
    public static decimal CalculateYieldRetention(decimal yieldAmount, decimal retentionRate, int precision)
    {
        return Math.Round(yieldAmount * retentionRate, precision);
    }

}
