using System.Collections.Generic;

namespace Operations.Integrations.Voids.RegisterVoidedTransactions;

public sealed record VoidedTransactionsValResult(
    IReadOnlyCollection<long> VoidIds,
    string Message,
    IReadOnlyCollection<VoidedTransactionFailure> FailedOperations);

public sealed record VoidedTransactionFailure(
    long ClientOperationId,
    string Code,
    string Message);
