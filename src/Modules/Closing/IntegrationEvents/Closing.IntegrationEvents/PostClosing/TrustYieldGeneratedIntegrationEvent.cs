using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents.PostClosing;

public sealed class TrustYieldGeneratedIntegrationEvent : IntegrationEvent
{
    public TrustYieldGeneratedIntegrationEvent(
        long trustId,
        int portfolioId,
        DateTime closingDate,
        decimal yieldAmount,
        decimal closingBalance,
        decimal yieldRetention,
        decimal units,
        DateTime processDate)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        TrustId = trustId;
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
        YieldAmount = yieldAmount;
        ClosingBalance = closingBalance;
        YieldRetention = yieldRetention;
        ProcessDate = processDate;
        Units = units;
    }

    public long TrustId { get; init; }
    public int PortfolioId { get; init; }
    public DateTime ClosingDate { get; init; }
    public decimal YieldAmount { get; init; }
    public decimal ClosingBalance { get; init; }
    public decimal YieldRetention { get; init; }
    public DateTime ProcessDate { get; init; }
    public decimal Units { get; init; }
}