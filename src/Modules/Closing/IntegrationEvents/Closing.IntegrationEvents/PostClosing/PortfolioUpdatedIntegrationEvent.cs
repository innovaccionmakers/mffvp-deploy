
using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents.PostClosing;

public sealed class PortfolioUpdatedIntegrationEvent : IntegrationEvent
{
    public PortfolioUpdatedIntegrationEvent(
        int portfolioId,
        DateTime closingDate)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
    }

    public int PortfolioId { get; init; }
    public DateTime ClosingDate { get; init; }

}