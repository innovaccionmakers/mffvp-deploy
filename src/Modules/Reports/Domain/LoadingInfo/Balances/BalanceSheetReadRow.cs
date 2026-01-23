namespace Reports.Domain.LoadingInfo.Balances;

public sealed record BalanceSheetReadRow(
    int AffiliateId,
    int PortfolioId,
    int GoalId,
    decimal Balance,
    decimal MinimumWages,
    int FundId,
    DateTime GoalCreatedAtUtc,
    int Age,
    bool IsDependent,
    decimal PortfolioEntries,
    decimal PortfolioWithdrawals,
    DateTime ClosingDateUtc
);
