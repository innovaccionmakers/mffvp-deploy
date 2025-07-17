namespace Trusts.Integrations.Trusts.GetBalances;

public sealed record BalanceResponse(
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    decimal AvailableAmount
);
