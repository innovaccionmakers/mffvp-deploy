using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustOperations.UpdateTrustOperation;

public sealed record UpdateTrustOperationCommand(
    Guid TrustOperationId,
    Guid NewCustomerDealId,
    Guid NewTrustId,
    decimal NewAmount
) : ICommand<TrustOperationResponse>;