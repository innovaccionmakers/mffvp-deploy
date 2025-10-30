using System;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;

namespace Trusts.Integrations.Trusts.PutTrust;

public sealed record PutTrustCommand(
    long ClientOperationId,
    LifecycleStatus Status,
    decimal TotalBalance,
    decimal TotalUnits,
    decimal Principal,
    decimal Earnings,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableAmount,
    DateTime UpdateDate) : ICommand;
