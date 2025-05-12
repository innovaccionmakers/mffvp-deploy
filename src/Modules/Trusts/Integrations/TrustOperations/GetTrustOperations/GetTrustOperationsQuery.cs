using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustOperations.GetTrustOperations;

public sealed record GetTrustOperationsQuery : IQuery<IReadOnlyCollection<TrustOperationResponse>>;