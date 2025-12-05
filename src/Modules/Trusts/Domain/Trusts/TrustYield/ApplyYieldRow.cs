
namespace Trusts.Domain.Trusts.TrustYield;
public sealed record ApplyYieldRow(
    long TrustId,
    decimal YieldAmount,
    decimal YieldRetentionRate,
    decimal ClosingBalance,
    decimal AgileWithdrawalPercentageProtectedBalance
);