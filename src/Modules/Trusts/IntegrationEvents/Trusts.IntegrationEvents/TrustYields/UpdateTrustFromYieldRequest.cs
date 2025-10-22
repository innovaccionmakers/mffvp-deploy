namespace Trusts.IntegrationEvents.TrustYields;

public sealed record UpdateTrustFromYieldRequest(
    int PortfolioId,
    DateTime ClosingDate,
    int BatchIndex,
    IReadOnlyList<ApplyYieldRowDto> Rows,
    string? IdempotencyKey = null
);