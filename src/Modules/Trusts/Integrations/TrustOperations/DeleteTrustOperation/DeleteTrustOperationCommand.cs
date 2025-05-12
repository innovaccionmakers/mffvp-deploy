using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustOperations.DeleteTrustOperation;

public sealed record DeleteTrustOperationCommand(
    Guid TrustOperationId
) : ICommand;