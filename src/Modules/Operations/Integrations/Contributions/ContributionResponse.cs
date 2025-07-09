using System.Text.Json.Serialization;

namespace Operations.Integrations.Contributions;

public sealed record ContributionResponse(
    [property: JsonPropertyName("IdOperacion")]
    long? OperationId,
    [property: JsonPropertyName("IdPortafolio")]
    string? PortfolioId,
    [property: JsonPropertyName("NombrePortafolio")]
    string? PortfolioName,
    [property: JsonPropertyName("CondicionTributaria")]
    string? TaxCondition,
    [property: JsonPropertyName("ValorRetencionContingente")]
    decimal? ContingentWithholdingValue,
    [property: JsonPropertyName("EnCola")]
    bool InQueue = false);