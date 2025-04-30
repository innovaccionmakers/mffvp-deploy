using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.TrustOperations.GetTrustOperation;
public sealed record GetTrustOperationQuery(
    Guid TrustOperationId
) : IQuery<TrustOperationResponse>;