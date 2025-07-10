namespace Treasury.Domain.TreasuryMovements;

public sealed record TreasuryMovementConceptSummary(
    long ConceptId,
    string ConceptName,
    string Nature,
    bool AllowsExpense,
    decimal TotalAmount
 );

