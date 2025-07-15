using Common.SharedKernel.Domain;

namespace Treasury.Integrations.TreasuryMovements.Response;
public sealed record GetMovementsByPortfolioIdResponse(
    long ConceptId,
    string ConceptName,
    IncomeExpenseNature Nature,
    bool AllowsExpense,
    decimal TotalAmount
    );