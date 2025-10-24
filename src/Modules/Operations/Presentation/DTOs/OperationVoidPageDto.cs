namespace Operations.Presentation.DTOs;

public sealed record OperationVoidPageDto(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyCollection<OperationVoidDto> Items);
