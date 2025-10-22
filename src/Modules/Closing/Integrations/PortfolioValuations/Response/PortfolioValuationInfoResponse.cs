namespace Closing.Integrations.PortfolioValuations.Response;

public sealed record PortfolioValuationInfoResponse(
    int PortfolioId,
    DateTime ClosingDate,
    decimal UnitValue);
