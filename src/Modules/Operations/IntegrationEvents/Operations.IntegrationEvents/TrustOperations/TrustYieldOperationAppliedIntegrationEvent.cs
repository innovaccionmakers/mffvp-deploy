using Common.SharedKernel.Application.EventBus;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed class TrustYieldOperationAppliedIntegrationEvent : IntegrationEvent
{
    public TrustYieldOperationAppliedIntegrationEvent(
        long trustId,
        int portfolioId,
        DateTime closingDate,
        decimal yieldAmount,
        decimal yieldRetention,
        decimal closingBalance)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        TrustId = trustId;
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
        YieldAmount = yieldAmount;
        YieldRetention = yieldRetention;
        ClosingBalance = closingBalance;
    }

    public long TrustId { get; init; }
    public int PortfolioId { get; init; }
    public DateTime ClosingDate { get; init; }
    public decimal YieldAmount { get; init; }
    public decimal YieldRetention { get; init; }
    public decimal ClosingBalance { get; init; }
}