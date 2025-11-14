
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;
using System.Text.Json;

namespace Common.SharedKernel.Application.Caching.OperationTypes;

public sealed record CachedOperationTypes(
    long Id,
    string Name,
    string? Category,
    IncomeEgressNature Nature,
    Status Status,
    string External,
    string HomologatedCode,
    JsonDocument AdditionalAttributes);