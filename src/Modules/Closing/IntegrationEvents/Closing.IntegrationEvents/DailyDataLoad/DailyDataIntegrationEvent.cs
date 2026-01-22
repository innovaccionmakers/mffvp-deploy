

using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents.DailyDataLoad;

public sealed class DailyDataIntegrationEvent(
    int portfolioId,
    DateTime closingDatetime) : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow)
{
    public int PortfolioId { get; init; } = portfolioId;
    public DateTime ClosingDatetime { get; init; } = closingDatetime;
}