using IntegrationResponse = Products.Integrations.Portfolios.GetPortfolioById;

namespace Products.IntegrationEvents.Portfolio;

public sealed record GetPortfolioByHomologatedCodeResponse(
    bool Succeeded,
    IntegrationResponse.GetPortfolioByIdResponse? Portfolio,
    string? Code,
    string? Message
);