using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.TrustSync;

public sealed record TrustSyncCommand(
    int TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    decimal PreClosingBalance,
    decimal Capital,
    decimal ContingentWithholding) : ICommand<bool>;
