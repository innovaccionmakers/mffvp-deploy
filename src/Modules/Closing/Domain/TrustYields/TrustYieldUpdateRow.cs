namespace Closing.Domain.TrustYields;
public sealed record TrustYieldUpdateRow(
    long TrustId,
    int PortfolioId,
    DateTime ClosingDateUtc,
    decimal Participation,
    decimal Units,
    decimal YieldAmount,
    decimal Income,
    decimal Expenses,
    decimal Commissions,
    decimal Cost,
    decimal ClosingBalance,
    DateTime ProcessDateUtc
);
