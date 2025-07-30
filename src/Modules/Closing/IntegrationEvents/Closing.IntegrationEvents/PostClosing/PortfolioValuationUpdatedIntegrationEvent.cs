
using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents.PostClosing;

public sealed class PortfolioValuationUpdatedIntegrationEvent : IntegrationEvent
{
    public PortfolioValuationUpdatedIntegrationEvent(
        int portfolioId,
        DateTime closingDate,
        decimal amount,
        decimal units,
        decimal unitValue,
        decimal grossYieldPerUnit,
        decimal costPerUnit,
        decimal dailyProfitability,
        decimal incomingOperations,
        decimal outgoingOperations,
        DateTime processDate)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
        Amount = amount;
        Units = units;
        UnitValue = unitValue;
        GrossYieldPerUnit = grossYieldPerUnit;
        CostPerUnit = costPerUnit;
        DailyProfitability = dailyProfitability;
        IncomingOperations = incomingOperations;
        OutgoingOperations = outgoingOperations;
        ProcessDate = processDate;
    }

    public int PortfolioId { get; init; }
    public DateTime ClosingDate { get; init; }
    public decimal Amount { get; init; }
    public decimal Units { get; init; }
    public decimal UnitValue { get; init; }
    public decimal GrossYieldPerUnit { get; init; }
    public decimal CostPerUnit { get; init; }
    public decimal DailyProfitability { get; init; }
    public decimal IncomingOperations { get; init; }
    public decimal OutgoingOperations { get; init; }
    public DateTime ProcessDate { get; init; }
}