using Common.SharedKernel.Core.Primitives;

namespace Trusts.IntegrationEvents.Trusts.GetTrustById;

public sealed record TrustDetails(
    long TrustId,
    int AffiliateId,
    long ClientOperationId,
    DateTime CreationDate,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    decimal TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableAmount,
    LifecycleStatus Status,
    DateTime? UpdateDate);
