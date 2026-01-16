namespace Products.IntegrationEvents.Portfolio.GetPortfolioInformation;

public sealed record GetPortfoliosBasicInformationByIdsRequest(
    IEnumerable<int> PortfolioIds
);

