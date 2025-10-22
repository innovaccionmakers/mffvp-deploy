using System;
using Common.SharedKernel.Core.Primitives;

namespace Trusts.IntegrationEvents.Trusts.PutTrust;

public sealed record PutTrustRequest(
    long ClientOperationId,
    LifecycleStatus Status,
    decimal TotalBalance,
    decimal Principal,
    decimal ContingentWithholding,
    DateTime UpdateDate);
