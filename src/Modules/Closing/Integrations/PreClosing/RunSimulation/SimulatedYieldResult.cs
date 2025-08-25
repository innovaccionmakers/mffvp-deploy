

using Closing.Integrations.Common;
using System.Text.Json.Serialization;

namespace Closing.Integrations.PreClosing.RunSimulation;

public sealed class SimulatedYieldResult
{
    [property: JsonPropertyName("Ingresos")]
    public decimal Income { get; init; }

    [property: JsonPropertyName("Egresos")]
    public decimal Expenses { get; init; }
    [property: JsonPropertyName("Comision")]
    public decimal Commissions { get; init; }
    [property: JsonPropertyName("Costos")]
    public decimal Costs { get; init; }
    [property: JsonPropertyName("RendimientosAbonar")]
    public decimal YieldToCredit { get; init; }

    // Simulación (no persistidos)
    [property: JsonPropertyName("ValorUnidad")]
    public decimal? UnitValue { get; init; }

    [property: JsonPropertyName("RentabilidadDiaria")]
    public decimal? DailyProfitability { get; init; }

    [property: JsonPropertyName("TieneAdvertencias")]
    public bool? HasWarnings { get; set;  }

    [property: JsonPropertyName("Advertencias")]
    public IReadOnlyList<WarningItem>? Warnings { get; set; }
}
