using Products.Integrations.Commissions.Response;

namespace Products.IntegrationEvents.Commission.CommissionsByPortfolio;

public sealed record CommissionsByPortfolioResponse
(   bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<GetCommissionsByPortfolioIdResponse> Commissions
);