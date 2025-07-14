using Common.SharedKernel.Domain;

namespace Treasury.Domain.TreasuryMovements;

public sealed record TreasuryMovementConceptSummary(
    long ConceptId,
    string ConceptName,
    IncomeExpenseNature Nature,
    bool AllowsExpense,
    decimal TotalAmount
 );

