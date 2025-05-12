using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustOperations.GetTrustOperation;

public sealed record GetTrustOperationQuery(
    Guid TrustOperationId
) : IQuery<TrustOperationResponse>;