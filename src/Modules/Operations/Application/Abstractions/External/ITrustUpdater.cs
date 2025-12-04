using System;
using System.Threading;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface ITrustUpdater
{
    Task<Result> UpdateAsync(TrustUpdate update, CancellationToken cancellationToken);
}

public sealed record TrustUpdate(
    long ClientOperationId,
    LifecycleStatus Status,
    decimal TotalBalance,
    decimal TotalUnits,
    decimal Principal,
    decimal Earnings,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableAmount,
    decimal ProtectedBalance,
    decimal AgileWithdrawalAvailable,
    DateTime UpdateDate);
