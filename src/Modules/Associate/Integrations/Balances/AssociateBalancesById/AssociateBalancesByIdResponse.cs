using System.Text.Json.Serialization;

namespace Associate.Integrations.Balances.AssociateBalancesById;

public sealed record AssociateBalancesByIdResponse(
    [property: JsonPropertyName("Sumatoria")] IReadOnlyCollection<AssociateBalanceItem> Sumatoria);
