

namespace Products.Integrations.Portfolios;

public sealed record PortfolioInfoForClosingResponse
(
     long PortfolioId,
     int AgileWithdrawalPercentageProtectedBalance
);
