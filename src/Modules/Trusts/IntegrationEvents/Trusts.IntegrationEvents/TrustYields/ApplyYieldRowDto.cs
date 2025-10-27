namespace Trusts.IntegrationEvents.TrustYields;

public sealed record ApplyYieldRowDto(
    long TrustId,
    decimal YieldAmount,
    decimal YieldRetention,
    decimal ClosingBalance
);
