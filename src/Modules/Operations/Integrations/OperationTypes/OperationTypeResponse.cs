using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;
using System.Text.Json;

namespace Operations.Integrations.OperationTypes;

public sealed record OperationTypeResponse(
    long OperationTypeId,
    string Name,
    string? Category,
    IncomeEgressNature Nature,
    Status Status,
    string External,
    string HomologatedCode,
    JsonDocument AdditionalAttributes);