using Common.SharedKernel.Domain;

namespace Reports.Domain.LoadingInfo.Balances;

public sealed class BalanceSheet : Entity
{
    public long Id { get; private set; }

    public long AffiliateId { get; private set; }
    public int PortfolioId { get; private set; }
    public int GoalId { get; private set; }

    public decimal Balance { get; private set; }
    public decimal MinimumWages { get; private set; }

    public int FundId { get; private set; }

    public DateTime GoalCreatedAtUtc { get; private set; }

    public int Age { get; private set; }
    public bool IsDependent { get; private set; }

    public decimal PortfolioEntries { get; private set; }
    public decimal PortfolioWithdrawals { get; private set; }

    public DateTime ClosingDateUtc { get; private set; }

    public static Result<BalanceSheet> Create(BalanceSheetReadRow row)
    {
        var entity = new BalanceSheet
        {
            Id = default,
            AffiliateId = row.AffiliateId,
            PortfolioId = row.PortfolioId,
            GoalId = row.GoalId,
            Balance = row.Balance,
            MinimumWages = row.MinimumWages,
            FundId = row.FundId,
            GoalCreatedAtUtc = row.GoalCreatedAtUtc,
            Age = row.Age,
            IsDependent = row.IsDependent,
            PortfolioEntries = row.PortfolioEntries,
            PortfolioWithdrawals = row.PortfolioWithdrawals,
            ClosingDateUtc = row.ClosingDateUtc
        };

        return Result.Success(entity);
    }
}