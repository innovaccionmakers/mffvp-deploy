using System;

namespace Operations.Integrations.ClientOperations.GetOperationsND;

public sealed record OperationNdItem(
    long ClientOperationId,
    DateTime ProcessDate,
    string TransactionTypeName,
    decimal Amount,
    decimal ContingentWithholding);
