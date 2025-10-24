using System;

namespace Operations.Integrations.ClientOperations.GetOperationsVoid;

public sealed record OperationVoidItem(
    long ClientOperationId,
    DateTime ProcessDate,
    string TransactionTypeName,
    long OperationTypeId,
    decimal Amount,
    decimal ContingentWithholding);
