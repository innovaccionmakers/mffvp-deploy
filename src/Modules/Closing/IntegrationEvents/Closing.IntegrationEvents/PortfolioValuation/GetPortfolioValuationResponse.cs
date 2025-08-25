using Closing.Integrations.PortfolioValuations.Response;

namespace Closing.Integrations.PortfolioValuation;

public sealed record GetPortfolioValuationResponse(
    bool IsValid,
    string? Code,
    string? Message,
    IReadOnlyCollection<PortfolioValuationResponse> PortfolioValuations
);
