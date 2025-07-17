using System.Text.Json.Serialization;

namespace Closing.Integrations.PortfolioValuations.Response;

public sealed record CheckValuationExistsResponse(
    [property: JsonPropertyName("Existe")]
    bool Exists
);