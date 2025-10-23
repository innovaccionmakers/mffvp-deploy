namespace Operations.Presentation.DTOs;

public sealed record OperationVoidDto(
    long ClientOperationId,
    DateTime ProcessDate,
    string TransactionTypeName,
    long OperationTypeId,
    decimal Amount,
    decimal ContingentWithholding);
