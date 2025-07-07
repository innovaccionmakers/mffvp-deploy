using Common.SharedKernel.Domain;
using System.Text.Json;

namespace Closing.Domain.YieldDetails;

public sealed class YieldDetail : Entity
{
    public long YieldDetailId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public string Source { get; private set; } = null!;
    public JsonDocument Concept { get; private set; } = null!;
    public decimal Income { get; private set; }
    public decimal Expenses { get; private set; }
    public decimal Commissions { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public bool IsClosed { get; private set; }

    private YieldDetail()
    {
    }

    public static Result<YieldDetail> Create(
        int portfolioId,
        DateTime closingDate,
        string source,
        JsonDocument concept,
        decimal income,
        decimal expenses,
        decimal commissions,
        DateTime processDate,
        bool isClosed)
    {
        var yieldDetail = new YieldDetail
        {
            YieldDetailId = default,
            PortfolioId = portfolioId,
            ClosingDate = closingDate,
            Source = source,
            Concept = concept,
            Income = income,
            Expenses = expenses,
            Commissions = commissions,
            ProcessDate = processDate,
            IsClosed = isClosed
        };

        return Result.Success(yieldDetail);
    }

    public void UpdateDetails(
        int portfolioId,
        DateTime closingDate,
        string source,
        JsonDocument concept,
        decimal income,
        decimal expenses,
        decimal commissions,
        DateTime processDate,
        bool isClosed)
    {
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
        Source = source;
        Concept = concept;
        Income = income;
        Expenses = expenses;
        Commissions = commissions;
        ProcessDate = processDate;
        IsClosed = isClosed;
    }
}