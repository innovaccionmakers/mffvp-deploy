namespace Contributions.Integrations.Trusts;

public sealed record TrustResponse(
    Guid TrustId,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    decimal? TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableBalance
);