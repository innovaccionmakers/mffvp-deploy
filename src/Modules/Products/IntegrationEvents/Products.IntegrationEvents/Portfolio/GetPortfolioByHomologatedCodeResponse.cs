using Products.Integrations.Portfolios;

namespace Products.IntegrationEvents.Portfolio;

public sealed record GetPortfolioByHomologatedCodeResponse(
    bool Succeeded,
    PortfolioResponse? Portfolio,
    string? Code,
    string? Message
);