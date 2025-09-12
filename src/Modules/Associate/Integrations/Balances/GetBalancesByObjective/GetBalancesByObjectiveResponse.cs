using System.Text.Json.Serialization;

namespace Associate.Integrations.Balances.GetBalancesByObjective;

public sealed record GetBalancesByObjectiveResponse(
    [property: JsonPropertyName("pageinfo")] PageInfo PageInfo,
    [property: JsonPropertyName("items")] IReadOnlyCollection<BalanceByObjectiveItem> Items
);
