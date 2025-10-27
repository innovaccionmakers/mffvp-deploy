namespace Operations.Presentation.DTOs;

public sealed record OperationNdPageDto(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyCollection<OperationNdDto> Items);
