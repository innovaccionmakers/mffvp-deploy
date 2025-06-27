using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;

namespace Products.Domain.PortfolioValuations;

public sealed class PortfolioValuation : Entity
{
    public int PortfolioValuationId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime CloseDate { get; private set; }
    public decimal Value { get; private set; }
    public decimal Units { get; private set; }
    public decimal UnitValue { get; private set; }
    public decimal GrossYieldUnits { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal DailyYield { get; private set; }
    public decimal IncomingOperations { get; private set; }
    public decimal OutgoingOperations { get; private set; }
    public DateTime ProcessDate { get; private set; }

    public Portfolio Portfolio { get; private set; }

    private PortfolioValuation()
    {
    }

    public static Result<PortfolioValuation> Create(
        int portfolioId,
        DateTime closeDate,
        decimal value,
        decimal units,
        decimal unitValue,
        decimal grossYieldUnits,
        decimal unitCost,
        decimal dailyYield,
        decimal incomingOperations,
        decimal outgoingOperations,
        DateTime processDate)
    {
        var portfolioValuation = new PortfolioValuation
        {
            PortfolioId = portfolioId,
            CloseDate = closeDate,
            Value = value,
            Units = units,
            UnitValue = unitValue,
            GrossYieldUnits = grossYieldUnits,
            UnitCost = unitCost,
            DailyYield = dailyYield,
            IncomingOperations = incomingOperations,
            OutgoingOperations = outgoingOperations,
            ProcessDate = processDate
        };

        return Result.Success(portfolioValuation);
    }

    public void UpdateValuation(
        DateTime newCloseDate,
        decimal newValue,
        decimal newUnits,
        decimal newUnitValue,
        decimal newGrossYieldUnits,
        decimal newUnitCost,
        decimal newDailyYield,
        decimal newIncomingOperations,
        decimal newOutgoingOperations,
        DateTime newProcessDate)
    {
        CloseDate = newCloseDate;
        Value = newValue;
        Units = newUnits;
        UnitValue = newUnitValue;
        GrossYieldUnits = newGrossYieldUnits;
        UnitCost = newUnitCost;
        DailyYield = newDailyYield;
        IncomingOperations = newIncomingOperations;
        OutgoingOperations = newOutgoingOperations;
        ProcessDate = newProcessDate;
    }
}