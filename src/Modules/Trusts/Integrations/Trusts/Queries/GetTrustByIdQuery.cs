using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;

namespace Trusts.Integrations.Trusts.Queries;

public sealed record GetTrustByIdQuery(long TrustId) : IQuery<TrustDetailsResult?>;

public sealed record TrustDetailsResult(
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
