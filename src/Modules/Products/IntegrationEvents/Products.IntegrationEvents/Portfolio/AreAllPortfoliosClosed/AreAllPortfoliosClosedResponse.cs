namespace Products.IntegrationEvents.Portfolio.AreAllPortfoliosClosed;

public sealed record AreAllPortfoliosClosedResponse(
    bool Succeeded,
    bool AreAllClosed,
    string? Code,
    string? Message
);

