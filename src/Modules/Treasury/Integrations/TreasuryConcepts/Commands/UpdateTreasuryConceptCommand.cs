using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Integrations.TreasuryConcepts.Response;

namespace Treasury.Integrations.TreasuryConcepts.Commands;

[AuditLog]
public sealed record UpdateTreasuryConceptCommand(
    long Id,
    string Concept,
    IncomeExpenseNature Nature,
    bool AllowsNegative,
    bool AllowsExpense,
    bool RequiresBankAccount,
    bool RequiresCounterparty,
    string? Observations = null
) : ICommand<TreasuryConceptResponse>;