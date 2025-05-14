namespace Trusts.Integrations.Trusts;

public sealed record TrustResponse(
    long TrustId,
    int AffiliateId,
    int ClientId,
    DateTime CreationDate,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    int TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableAmount,
    decimal ContingentWithholdingPercentage
);