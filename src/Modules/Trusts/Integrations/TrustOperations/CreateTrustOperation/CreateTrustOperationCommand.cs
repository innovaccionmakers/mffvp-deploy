using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustOperations.CreateTrustOperation;

public sealed record CreateTrustOperationCommand(
    Guid CustomerDealId,
    Guid TrustId,
    decimal Amount
) : ICommand<TrustOperationResponse>;