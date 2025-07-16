using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Integrations.TreasuryConcepts.Response;

namespace Treasury.Integrations.TreasuryConcepts.Commands;

public sealed record CreateTreasuryConceptCommand(
    string Concept,
    IncomeExpenseNature Nature,
    bool AllowsNegative,
    bool AllowsExpense,
    bool RequiresBankAccount,
    bool RequiresCounterparty,
    DateTime ProcessDate,
    string? Observations = null
) : ICommand<TreasuryConceptResponse>;
