using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.TrustSync;
public sealed record TrustSyncPostCommand(
    int PortfolioId,
    DateTime ClosingDate
    ) : ICommand<bool>;
