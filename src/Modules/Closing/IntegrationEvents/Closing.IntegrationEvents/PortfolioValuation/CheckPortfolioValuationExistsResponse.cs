namespace Closing.IntegrationEvents.PortfolioValuation;

public record CheckPortfolioValuationExistsResponse(
    bool Succeeded,
    bool Exists,
    string? Code,
    string? Message
);
