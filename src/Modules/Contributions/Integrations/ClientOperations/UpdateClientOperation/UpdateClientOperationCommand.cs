using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.ClientOperations.UpdateClientOperation;
public sealed record UpdateClientOperationCommand(
    Guid ClientOperationId,
    DateTime NewDate,
    int NewAffiliateId,
    int NewObjectiveId,
    int NewPortfolioId,
    int NewTransactionTypeId,
    int NewSubTransactionTypeId,
    decimal NewAmount
) : ICommand<ClientOperationResponse>;