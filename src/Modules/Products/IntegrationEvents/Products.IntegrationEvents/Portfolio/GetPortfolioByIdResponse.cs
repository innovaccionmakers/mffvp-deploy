using Products.Integrations.Portfolios;

namespace Products.IntegrationEvents.Portfolio;

public sealed record GetPortfolioByIdResponse(
    bool Succeeded,
    PortfolioResponse? Portfolio,
    string? Code,
    string? Message
);
