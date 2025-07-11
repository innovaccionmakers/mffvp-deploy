namespace Treasury.Integrations.TreasuryMovements.Response;
public sealed record GetMovementsByPortfolioIdResponse(
    long ConceptId,
    string ConceptName,
    string Nature,
    bool AllowsExpense,
    decimal TotalAmount
    );