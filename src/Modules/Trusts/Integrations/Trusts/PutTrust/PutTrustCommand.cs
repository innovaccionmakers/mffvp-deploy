using System;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;

namespace Trusts.Integrations.Trusts.PutTrust;

[AuditLog]
public sealed record PutTrustCommand(
    long ClientOperationId,
    LifecycleStatus Status,
    decimal TotalBalance,
    decimal Principal,
    decimal ContingentWithholding,
    DateTime UpdateDate) : ICommand;
