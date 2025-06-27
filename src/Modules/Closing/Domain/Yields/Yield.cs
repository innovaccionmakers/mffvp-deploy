using Common.SharedKernel.Domain;

namespace Closing.Domain.Yields;

public sealed class Yield : Entity
{
    public long YieldId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal Income { get; private set; }
    public decimal Expenses { get; private set; }
    public decimal Commissions { get; private set; }
    public decimal Costs { get; private set; }
    public decimal YieldToCredit { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public bool IsClosed { get; private set; }

    public ICollection<YieldDetails.YieldDetail> YieldDetails { get; private set; } = [];

    private Yield()
    {
    }

    public static Result<Yield> Create(
        int portfolioId,
        decimal income,
        decimal expenses,
        decimal commissions,
        decimal costs,
        decimal yieldToCredit,
        DateTime closingDate,
        DateTime processDate,
        bool isClosed)
    {
        var yield = new Yield
        {
            YieldId = default,
            PortfolioId = portfolioId,
            Income = income,
            Expenses = expenses,
            Commissions = commissions,
            Costs = costs,
            YieldToCredit = yieldToCredit,
            ClosingDate = closingDate,
            ProcessDate = processDate,
            IsClosed = isClosed
        };

        return Result.Success(yield);
    }

    public void UpdateDetails(
        int portfolioId,
        decimal income,
        decimal expenses,
        decimal commissions,
        decimal costs,
        decimal yieldToCredit,
        DateTime closingDate,
        DateTime processDate,
        bool isClosed)
    {
        PortfolioId = portfolioId;
        Income = income;
        Expenses = expenses;
        Commissions = commissions;
        Costs = costs;
        YieldToCredit = yieldToCredit;
        ClosingDate = closingDate;
        ProcessDate = processDate;
        IsClosed = isClosed;
    }
}