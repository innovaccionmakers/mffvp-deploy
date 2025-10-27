namespace Operations.Presentation.DTOs;

public sealed record OperationNdDto(
    long ClientOperationId,
    DateTime ProcessDate,
    string TransactionTypeName,
    decimal Amount,
    decimal ContingentWithholding);
