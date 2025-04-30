using Common.SharedKernel.Domain;

namespace Contributions.Domain.Trusts;
public sealed class Trust : Entity
{
    public Guid TrustId { get; private set; }
    public int AffiliateId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal TotalBalance { get; private set; }
    public decimal? TotalUnits { get; private set; }
    public decimal Principal { get; private set; }
    public decimal Earnings { get; private set; }
    public int TaxCondition { get; private set; }
    public decimal ContingentWithholding { get; private set; }
    public decimal EarningsWithholding { get; private set; }
    public decimal AvailableBalance { get; private set; }

    private Trust() { }

    public static Result<Trust> Create(
        int affiliateid, int objectiveid, int portfolioid, decimal totalbalance, decimal? totalunits, decimal principal, decimal earnings, int taxcondition, decimal contingentwithholding, decimal earningswithholding, decimal availablebalance
    )
    {
        var trust = new Trust
        {
                TrustId = Guid.NewGuid(),
                AffiliateId = affiliateid,
                ObjectiveId = objectiveid,
                PortfolioId = portfolioid,
                TotalBalance = totalbalance,
                TotalUnits = totalunits,
                Principal = principal,
                Earnings = earnings,
                TaxCondition = taxcondition,
                ContingentWithholding = contingentwithholding,
                EarningsWithholding = earningswithholding,
                AvailableBalance = availablebalance,
        };
        trust.Raise(new TrustCreatedDomainEvent(trust.TrustId));
        return Result.Success(trust);
    }

    public void UpdateDetails(
        int newAffiliateId, int newObjectiveId, int newPortfolioId, decimal newTotalBalance, decimal? newTotalUnits, decimal newPrincipal, decimal newEarnings, int newTaxCondition, decimal newContingentWithholding, decimal newEarningsWithholding, decimal newAvailableBalance
    )
    {
        AffiliateId = newAffiliateId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        TotalBalance = newTotalBalance;
        TotalUnits = newTotalUnits;
        Principal = newPrincipal;
        Earnings = newEarnings;
        TaxCondition = newTaxCondition;
        ContingentWithholding = newContingentWithholding;
        EarningsWithholding = newEarningsWithholding;
        AvailableBalance = newAvailableBalance;
    }
}