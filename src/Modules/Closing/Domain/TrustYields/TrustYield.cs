using Common.SharedKernel.Domain;

namespace Closing.Domain.TrustYields;

public sealed class TrustYield : Entity
{
    public long TrustYieldId { get; private set; }
    public int TrustId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public decimal Participation { get; private set; }
    public decimal Units { get; private set; }
    public decimal YieldAmount { get; private set; }
    public decimal PreClosingBalance { get; private set; }
    public decimal ClosingBalance { get; private set; }
    public decimal Income { get; private set; }
    public decimal Expenses { get; private set; }
    public decimal Commissions { get; private set; }
    public decimal Cost { get; private set; }
    public decimal Capital { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public decimal ContingentRetention { get; private set; }
    public decimal YieldRetention { get; private set; }

    private TrustYield()
    {
    }

    public static Result<TrustYield> Create(
        int trustId,
        int portfolioId,
        DateTime closingDate,
        decimal participation,
        decimal units,
        decimal yieldAmount,
        decimal preClosingBalance,
        decimal closingBalance,
        decimal income,
        decimal expenses,
        decimal commissions,
        decimal cost,
        decimal capital,
        DateTime processDate,
        decimal contingentRetention,
        decimal yieldRetention)
    {
        var trustYield = new TrustYield
        {
            TrustYieldId = default,
            TrustId = trustId,
            PortfolioId = portfolioId,
            ClosingDate = closingDate,
            Participation = participation,
            Units = units,
            YieldAmount = yieldAmount,
            PreClosingBalance = preClosingBalance,
            ClosingBalance = closingBalance,
            Income = income,
            Expenses = expenses,
            Commissions = commissions,
            Cost = cost,
            Capital = capital,
            ProcessDate = processDate,
            ContingentRetention = contingentRetention,
            YieldRetention = yieldRetention
        };

        return Result.Success(trustYield);
    }

    public void UpdateDetails(
        int trustId,
        int portfolioId,
        DateTime closingDate,
        decimal participation,
        decimal units,
        decimal yieldAmount,
        decimal preClosingBalance,
        decimal closingBalance,
        decimal income,
        decimal expenses,
        decimal commissions,
        decimal cost,
        decimal capital,
        DateTime processDate,
        decimal contingentRetention,
        decimal yieldRetention)
    {
        TrustId = trustId;
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
        Participation = participation;
        Units = units;
        YieldAmount = yieldAmount;
        PreClosingBalance = preClosingBalance;
        ClosingBalance = closingBalance;
        Income = income;
        Expenses = expenses;
        Commissions = commissions;
        Cost = cost;
        Capital = capital;
        ProcessDate = processDate;
        ContingentRetention = contingentRetention;
        YieldRetention = yieldRetention;
    }
}