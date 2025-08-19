namespace Common.SharedKernel.Application.Helpers.Finance;

/// <summary>
/// Contiene funciones financieras comunes para rentabilidad, tasas y valorizaciones.
/// </summary>
public static class PortfolioMath

{
    // -------------------------
    // UNIDAD
    // -------------------------

    /// <summary>
    /// Calcula valor de unidad en función del valor anterior del portafolio más el rendimiento abonado.
    /// </summary>
    public static decimal CalculateUnitValue(decimal previousPortfolioValue, decimal yieldToCredit, decimal units)
    {
        if (units <= 0) return 0;
        return (previousPortfolioValue + yieldToCredit) / units;
    }

    /// <summary>
    /// Calcula el nuevo valor de la unidad con redondeo.
    /// </summary>
    public static decimal CalculateRoundedUnitValue(decimal previousPortfolioValue, decimal yieldToCredit, decimal units, int precision)
    {
        if (units <= 0) return 0;
        return Math.Round(CalculateUnitValue(previousPortfolioValue, yieldToCredit, units), precision);
    }

    /// <summary>
    /// Calcula la cantidad de nuevas unidades del portafolio.
    /// </summary>
    public static decimal CalculateNewUnits(decimal newPortfolioValue, decimal newUnitValue, int precision)
    {
        if (newUnitValue <= 0) return 0;
        return Math.Round(newPortfolioValue / newUnitValue, precision);
    }

    /// <summary>
    /// Calcula el rendimiento bruto por unidad a partir de los ingresos.
    /// </summary>
    public static decimal CalculateGrossYieldPerUnitFromIncome(decimal yieldIncome, decimal previousUnits, int precision)
    {
        if (previousUnits <= 0) return 0;
        return Math.Round(yieldIncome / previousUnits, precision);
    }

    /// <summary>
    /// Calcula el costo por unidad.
    /// </summary>
    public static decimal CalculateCostPerUnit(decimal yieldCosts, decimal previousUnits, int precision)
    {
        if (previousUnits <= 0) return 0;
        return Math.Round(yieldCosts / previousUnits, precision);
    }

    // -------------------------
    // PORTAFOLIO
    // -------------------------

    /// <summary>
    /// Calcula el nuevo valor del portafolio sumando el valor anterior, el rendimiento, 
    /// las operaciones de entrada y restando operaciones de salida.
    /// Permite especificar la precisión (cantidad de decimales) del resultado.
    /// </summary>
    /// <param name="previousValue">Valor del portafolio en el día anterior.</param>
    /// <param name="yieldToCredit">Rendimiento a acreditar.</param>
    /// <param name="incoming">Operaciones de entrada.</param>
    /// <param name="outgoing">Operaciones de salida.</param>
    /// <param name="precision">
    /// Cantidad de decimales a devolver en el resultado (por defecto: 2).
    /// </param>
    /// <returns>Nuevo valor del portafolio redondeado a la precisión indicada.</returns>
    public static decimal CalculateNewPortfolioValue(
        decimal previousValue,
        decimal yieldToCredit,
        decimal incoming,
        decimal outgoing,
        int precision = 2)
    {
        var result = previousValue + yieldToCredit + incoming - outgoing;
        return Math.Round(result, precision, MidpointRounding.AwayFromZero);
    }


    // -------------------------
    // RENTABILIDAD Y TASAS
    // -------------------------

    /// <summary>
    /// Calcula rentabilidad compuesta anual (por ejemplo, rentabilidad diaria anualizada).
    /// </summary>
    public static decimal CalculateCompoundReturn(decimal baseValue, decimal newValue, int periodsPerYear)
    {
        if (baseValue <= 0) return 0;
        return (decimal)Math.Pow((double)(newValue / baseValue), periodsPerYear) - 1;
    }

    /// <summary>
    /// Calcula la rentabilidad diaria compuesta con redondeo.
    /// </summary>
    public static decimal CalculateRoundedDailyProfitability(decimal previousUnitValue, decimal newUnitValue, int precision, int periodsPerYear = 365)
    {
        if (previousUnitValue <= 0) return 0;
        var profitability = CalculateCompoundReturn(previousUnitValue, newUnitValue, periodsPerYear);
        return Math.Round(profitability, precision);
    }

    /// <summary>
    /// Calcula la rentabilidad diaria simple.
    /// </summary>
    public static decimal CalculateDailyProfitability(decimal previousUnitValue, decimal currentUnitValue)
    {
        if (previousUnitValue <= 0) return 0;
        return (currentUnitValue / previousUnitValue) - 1;
    }

    /// <summary>
    /// Calcula el rendimiento bruto por unidad a partir de la rentabilidad diaria y el valor de unidad.
    /// </summary>
    public static decimal CalculateGrossYieldPerUnitFromProfitability(decimal unitValue, decimal dailyProfitability)
    {
        return unitValue * dailyProfitability;
    }

    /// <summary>
    /// Calcula la tasa efectiva anual (TEA) a partir de la rentabilidad diaria.
    /// </summary>
    public static decimal CalculateAnnualEffectiveRate(decimal dailyProfitability, int workingDaysPerYear = 252)
    {
        return (decimal)Math.Pow(1 + (double)dailyProfitability, workingDaysPerYear) - 1;
    }
}
