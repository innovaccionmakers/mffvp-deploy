namespace Products.IntegrationEvents.Portfolio.AreAllPortfoliosClosed;

public sealed record AreAllPortfoliosClosedRequest(
    IEnumerable<int> PortfolioIds,
    DateTime Date
);

