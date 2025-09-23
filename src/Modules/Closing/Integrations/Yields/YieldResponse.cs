namespace Closing.Integrations.Yields;

public sealed record YieldResponse
(
    long YieldId,
    int PortfolioId,
    decimal Income,
    decimal Expenses,
    decimal Commissions,
    decimal Costs,
    decimal YieldToCredit,
    decimal CreditedYields,
    DateTime ClosingDate,
    DateTime ProcessDate,
    bool IsClosed
);
