using System.Collections.Generic;

namespace Operations.Integrations.ClientOperations.GetOperationsVoid;

public sealed record GetOperationsVoidResponse(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyCollection<OperationVoidItem> Items);
