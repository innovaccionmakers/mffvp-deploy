

namespace Products.IntegrationEvents.Portfolio.GetInfoForClosing;

public sealed record GetPortfolioInfoForClosingResponse
(
    bool Succeeded,
    int AgileWithdrawalPercentageProtectedBalance,
    string? Code,
    string? Message
);
