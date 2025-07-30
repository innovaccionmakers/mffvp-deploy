using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents.PostClosing;

public sealed class CommissionProcessedIntegrationEvent : IntegrationEvent
{
    public CommissionProcessedIntegrationEvent(
        int portfolioId,
        int commissionId,
        decimal accumulatedValue,
        DateTime closingDate)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        PortfolioId = portfolioId;
        CommissionId = commissionId;
        AccumulatedValue = accumulatedValue;
        ClosingDate = closingDate;
    }

    public int PortfolioId { get; init; }
    public int CommissionId { get; init; }
    public decimal AccumulatedValue { get; init; }
    public DateTime ClosingDate { get; init; }
}