using Trusts.Integrations.Trusts.GetBalances;

namespace Trusts.IntegrationEvents.GetBalances;

public sealed record GetBalancesResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<BalanceResponse> Balances);
