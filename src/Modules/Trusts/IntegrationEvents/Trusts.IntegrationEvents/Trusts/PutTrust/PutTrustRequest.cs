using System;
using Common.SharedKernel.Core.Primitives;

namespace Trusts.IntegrationEvents.Trusts.PutTrust;

public sealed record PutTrustRequest(
    long ClientOperationId,
    LifecycleStatus Status,
    decimal TotalBalance,
    decimal TotalUnits,
    decimal Principal,
    decimal Earnings,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableAmount,
    DateTime UpdateDate);
