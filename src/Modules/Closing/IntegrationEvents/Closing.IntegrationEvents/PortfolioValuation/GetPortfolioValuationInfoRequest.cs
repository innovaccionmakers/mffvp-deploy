namespace Closing.IntegrationEvents.PortfolioValuation;

public sealed record GetPortfolioValuationInfoRequest(int PortfolioId, DateTime ClosingDate);
