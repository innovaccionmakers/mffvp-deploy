using Common.SharedKernel.Domain;

namespace Closing.Domain.PortfolioValuations;

public sealed class PortfolioValuation : Entity
{
    public long PortfolioValuationId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public decimal Amount { get; private set; }
    public decimal InitialValue { get; private set; }
    public decimal Units { get; private set; }
    public decimal UnitValue { get; private set; }
    public decimal GrossYieldPerUnit { get; private set; }
    public decimal CostPerUnit { get; private set; }
    public decimal DailyProfitability { get; private set; }
    public decimal IncomingOperations { get; private set; }
    public decimal OutgoingOperations { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public bool IsClosed { get; private set; }

    private PortfolioValuation()
    {
    }

    public static Result<PortfolioValuation> Create(
        int portfolioId,
        DateTime closingDate,
        decimal amount,
        decimal initialValue,
        decimal units,
        decimal unitValue,
        decimal grossYieldPerUnit,
        decimal costPerUnit,
        decimal dailyProfitability,
        decimal incomingOperations,
        decimal outgoingOperations,
        DateTime processDate,
        bool isClosed)
    {
        var portfolioValuation = new PortfolioValuation
        {
            PortfolioValuationId = default,
            PortfolioId = portfolioId,
            ClosingDate = closingDate,
            Amount = amount,
            InitialValue = initialValue,
            Units = units,
            UnitValue = unitValue,
            GrossYieldPerUnit = grossYieldPerUnit,
            CostPerUnit = costPerUnit,
            DailyProfitability = dailyProfitability,
            IncomingOperations = incomingOperations,
            OutgoingOperations = outgoingOperations,
            ProcessDate = processDate,
            IsClosed = isClosed
        };

        return Result.Success(portfolioValuation);
    }

    public void UpdateDetails(
        int portfolioId,
        DateTime closingDate,
        decimal amount,
        decimal initialValue,
        decimal units,
        decimal unitValue,
        decimal grossYieldPerUnit,
        decimal costPerUnit,
        decimal dailyProfitability,
        decimal incomingOperations,
        decimal outgoingOperations,
        DateTime processDate,
        bool isClosed)
    {
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
        Amount = amount;
        InitialValue = initialValue;
        Units = units;
        UnitValue = unitValue;
        GrossYieldPerUnit = grossYieldPerUnit;
        CostPerUnit = costPerUnit;
        DailyProfitability = dailyProfitability;
        IncomingOperations = incomingOperations;
        OutgoingOperations = outgoingOperations;
        ProcessDate = processDate;
        IsClosed = isClosed;
    }
}