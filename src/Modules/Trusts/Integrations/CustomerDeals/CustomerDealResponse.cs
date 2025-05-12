namespace Trusts.Integrations.CustomerDeals;

public sealed record CustomerDealResponse(
    Guid CustomerDealId,
    DateTime Date,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    int ConfigurationParamId,
    decimal Amount
);