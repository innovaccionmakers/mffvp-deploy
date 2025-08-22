using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;

namespace Operations.Integrations.OperationTypes;

public sealed record OperationTypeResponse(
    long OperationTypeId,
    string Name,
    string? Category,
    IncomeEgressNature Nature,
    Status Status,
    string External,
    string HomologatedCode);