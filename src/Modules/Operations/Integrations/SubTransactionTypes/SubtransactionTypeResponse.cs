using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;

namespace Operations.Integrations.SubTransactionTypes;

public sealed record SubtransactionTypeResponse(
    long SubtransactionTypeId,
    string Name,
    string Category,
    IncomeEgressNature Nature,
    Status Status,
    string External,
    string HomologatedCode);