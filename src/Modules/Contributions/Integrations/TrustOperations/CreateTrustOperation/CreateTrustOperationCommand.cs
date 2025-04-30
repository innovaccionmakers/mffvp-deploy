using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.TrustOperations.CreateTrustOperation;
public sealed record CreateTrustOperationCommand(
    Guid ClientOperationId,
    Guid TrustId,
    decimal Amount
) : ICommand<TrustOperationResponse>;