using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustYields.Commands;

public sealed record UpdateTrustFromYieldCommand(
    long TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    decimal YieldAmount,
    decimal YieldRetention,
    decimal ClosingBalance,
    decimal Units
) : ICommand;