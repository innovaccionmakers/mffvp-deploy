using System.Collections.Generic;

namespace Operations.Integrations.ClientOperations.GetOperationsND;

public sealed record GetOperationsNDResponse(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyCollection<OperationNdItem> Items);
