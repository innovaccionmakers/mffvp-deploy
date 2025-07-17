namespace Trusts.Domain.Trusts.Balances;

public sealed class AffiliateBalance
{
    public int ObjectiveId { get; set; }
    public int PortfolioId { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableAmount { get; set; }

    public AffiliateBalance() { }

    public static AffiliateBalance Create(int objectiveId, int portfolioId, decimal totalBalance, decimal availableAmount)
    {
        return new AffiliateBalance
        {
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            TotalBalance = totalBalance,
            AvailableAmount = availableAmount
        };
    }
}
