namespace Closing.Integrations.YieldDetails;

public sealed record YieldDetailResponse
(
    long YieldDetailId,
    int PortfolioId,
    decimal Income,
    decimal Expenses,
    decimal Commissions,
    DateTime ClosingDate,
    DateTime ProcessDate,
    bool IsClosed
);

