using Products.Integrations.Portfolios;

namespace Products.IntegrationEvents.Portfolio.GetPortfolioInformation;

public sealed record GetPortfolioInformationByIdResponse(
    bool Succeeded,
    CompletePortfolioInformationResponse? PortfolioInformation,
    string? Code,
    string? Message
);
