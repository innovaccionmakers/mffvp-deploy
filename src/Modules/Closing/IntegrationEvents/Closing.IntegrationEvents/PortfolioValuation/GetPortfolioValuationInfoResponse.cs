using System;

namespace Closing.IntegrationEvents.PortfolioValuation;

public sealed record GetPortfolioValuationInfoResponse(
    bool IsValid,
    string? Code,
    string? Message,
    PortfolioValuationInfoDto? ValuationInfo);

public sealed record PortfolioValuationInfoDto(
    int PortfolioId,
    DateTime ClosingDate,
    decimal UnitValue);
