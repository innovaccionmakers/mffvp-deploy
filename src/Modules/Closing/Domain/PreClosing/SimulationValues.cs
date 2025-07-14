namespace Closing.Domain.PreClosing;

/// <summary>
/// Resultado del cálculo simulado de rendimientos financieros.
/// </summary>
public sealed record SimulationValues(
    decimal? UnitValue,
    decimal? DailyProfitability
);
