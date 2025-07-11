using System.Text.Json.Serialization;

namespace Closing.Integrations.ProfitLosses.GetProfitandLoss;

public sealed record GetProfitandLossResponse(
    [property: JsonPropertyName("Conceptos")]
    IReadOnlyDictionary<string, decimal> Values,
    [property: JsonPropertyName("RendimientosNetos")]
    decimal NetYield
);