

namespace Trusts.IntegrationEvents.TrustYields;

public sealed record UpdateTrustFromYieldRequest(
      int PortfolioId,
      DateTime ClosingDate,
      long TrustId,
      decimal YieldAmount,
      decimal YieldRetention,
      decimal ClosingBalance
  // , string? CorrelationId = null
  );