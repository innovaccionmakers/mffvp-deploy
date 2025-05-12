using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.GetTrust;

public sealed record GetTrustQuery(
    Guid TrustId
) : IQuery<TrustResponse>;