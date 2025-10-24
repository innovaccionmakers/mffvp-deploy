using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.TrustInfo;

public sealed record TrustInfoQuery(long ClientOperationId, decimal ContributionValue)
    : IQuery<TrustInfoQueryResponse>;
