using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;

namespace Operations.Integrations.OperationTypes;

public sealed record OperationTypeResponse(
    long OperationTypeId,
    string Name,
    string? Category,
    IncomeEgressNature Nature,
    Status Status,
    string External,
    string HomologatedCode);