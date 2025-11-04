using System.Collections.Generic;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.Voids.RegisterVoidedTransactions;

[AuditLog]
public sealed record VoidedTransactionsValCommand(
    IReadOnlyCollection<VoidedTransactionItem> Items,
    long CauseId,
    int AffiliateId,
    int ObjectiveId
) : ICommand<VoidedTransactionsValResult>;

public sealed record VoidedTransactionItem(long ClientOperationId, decimal Amount);
