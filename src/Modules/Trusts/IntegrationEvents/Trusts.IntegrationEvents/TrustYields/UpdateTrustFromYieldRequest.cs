namespace Trusts.IntegrationEvents.TrustYields;

public sealed record UpdateTrustFromYieldRequest(
    int PortfolioId,
    DateTime ClosingDate,
    int BatchIndex,
    IReadOnlyList<ApplyYieldRowDto> Rows,
    decimal AgileWithdrawalPercentageProtectedBalance,
    string? IdempotencyKey = null
);