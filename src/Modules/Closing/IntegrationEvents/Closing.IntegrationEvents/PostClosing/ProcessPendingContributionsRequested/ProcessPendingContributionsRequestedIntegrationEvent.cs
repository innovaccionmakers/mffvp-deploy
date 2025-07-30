using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents.PostClosing.ProcessPendingContributionsRequested;

public sealed class ProcessPendingContributionsRequestedIntegrationEvent(int portfolioId)
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow)
{
    public int PortfolioId { get; init; } = portfolioId;
}
