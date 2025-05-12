namespace Trusts.Integrations.Trusts;

public sealed record TrustResponse(
    Guid TrustId,
    int AffiliateId,
    int ClientId,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    int TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    int ContingentWithholding
);