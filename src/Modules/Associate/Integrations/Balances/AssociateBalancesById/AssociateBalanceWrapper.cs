using System.Text.Json.Serialization;

namespace Associate.Integrations.Balances.AssociateBalancesById;

public sealed record AssociateBalanceWrapper(
    [property: JsonPropertyName("Saldo")] AssociateBalanceItem Saldo);
