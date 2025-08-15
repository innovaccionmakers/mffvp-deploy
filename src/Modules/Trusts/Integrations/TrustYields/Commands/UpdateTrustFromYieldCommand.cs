using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustYields.Commands;

[AuditLog]
public sealed record UpdateTrustFromYieldCommand(
    long TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    decimal YieldAmount,
    decimal YieldRetention,
    decimal ClosingBalance,
    decimal Units
) : ICommand;