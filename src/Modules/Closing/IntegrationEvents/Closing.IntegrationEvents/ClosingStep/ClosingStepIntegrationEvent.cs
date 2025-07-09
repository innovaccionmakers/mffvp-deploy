using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents.ClosingStep;

public sealed class ClosingStepIntegrationEvent(
    int portfolioId,
    DateTime closingDatetime,
    DateTime processDatetime,
    string process)
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow)
{
    public int PortfolioId { get; init; } = portfolioId;
    public DateTime ClosingDatetime { get; init; } = closingDatetime;
    public DateTime ProcessDatetime { get; init; } = processDatetime;
    public string Process { get; init; } = process;
}
