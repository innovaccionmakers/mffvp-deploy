
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;

namespace Common.SharedKernel.Application.Caching.OperationTypes;

public sealed record CachedOperationTypes(
    long Id,
    string Name,
    string? Category,
    IncomeEgressNature Nature,
    Status Status,
    string External,
    string HomologatedCode);