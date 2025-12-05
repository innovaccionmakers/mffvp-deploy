using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;

namespace Trusts.Integrations.Trusts.CreateTrust;

public sealed record CreateTrustCommand(
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
    decimal ProtectedBalance,
    decimal AgileWithdrawalAvailable,
    LifecycleStatus Status
) : ICommand<TrustResponse>;