using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.TrustOperations.DeleteTrustOperation;
public sealed record DeleteTrustOperationCommand(
    Guid TrustOperationId
) : ICommand;