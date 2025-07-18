namespace Closing.Application.PreClosing.Services.Yield.Dto;

/// <summary>
/// Resultado del cálculo simulado de rendimientos financieros.
/// </summary>
public sealed record SimulationValues(
    decimal? UnitValue,
    decimal? DailyProfitability
);
