using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.ClientOperations.CreateClientOperation;
public sealed record CreateClientOperationCommand(
    DateTime Date,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    int TransactionTypeId,
    int SubTransactionTypeId,
    decimal Amount
) : ICommand<ClientOperationResponse>;