using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.GetBalances;

public sealed record GetBalancesQuery(int AffiliateId) : IQuery<IReadOnlyCollection<BalanceResponse>>;
