using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.TrustOperations.UpdateTrustOperation;
public sealed record UpdateTrustOperationCommand(
    Guid TrustOperationId,
    Guid NewClientOperationId,
    Guid NewTrustId,
    decimal NewAmount
) : ICommand<TrustOperationResponse>;