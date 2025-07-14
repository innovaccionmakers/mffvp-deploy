

namespace Closing.Integrations.PreClosing.RunSimulation;

public sealed class SimulatedYieldResult
{
    public decimal Income { get; init; }
    public decimal Expenses { get; init; }
    public decimal Commissions { get; init; }
    public decimal Costs { get; init; }
    public decimal YieldToCredit { get; init; }

    // Simulación (no persistidos)
    public decimal? UnitValue { get; init; }
    public decimal? DailyProfitability { get; init; }
}
