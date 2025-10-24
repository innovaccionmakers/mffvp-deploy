using System.Collections.Generic;

namespace Operations.Integrations.Voids.RegisterVoidedTransactions;

public sealed record VoidedTransactionsValResult(
    IReadOnlyCollection<long> VoidIds,
    string Message,
    IReadOnlyCollection<VoidedTransactionFailure> FailedOperations,
    int TotalProcessed,
    int SuccessCount,
    int ErrorCount);

public sealed record VoidedTransactionFailure(
    long ClientOperationId,
    string Code,
    string Message);
