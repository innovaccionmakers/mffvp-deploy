using Common.SharedKernel.Domain;

namespace Trusts.Domain.Trusts;

public sealed class Trust : Entity
{
    private Trust()
    {
    }

    public Guid TrustId { get; private set; }
    public int AffiliateId { get; private set; }
    public int ClientId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal TotalBalance { get; private set; }
    public int TotalUnits { get; private set; }
    public decimal Principal { get; private set; }
    public decimal Earnings { get; private set; }
    public int TaxCondition { get; private set; }
    public int ContingentWithholding { get; private set; }

    public static Result<Trust> Create(
        int affiliateId, int clientId, int objectiveId, int portfolioId, decimal totalBalance, int totalUnits,
        decimal principal, decimal earnings, int taxCondition, int contingentWithholding
    )
    {
        var trust = new Trust
        {
            TrustId = Guid.NewGuid(),
            AffiliateId = affiliateId,
            ClientId = clientId,
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            TotalBalance = totalBalance,
            TotalUnits = totalUnits,
            Principal = principal,
            Earnings = earnings,
            TaxCondition = taxCondition,
            ContingentWithholding = contingentWithholding
        };
        trust.Raise(new TrustCreatedDomainEvent(trust.TrustId));
        return Result.Success(trust);
    }

    public void UpdateDetails(
        int newAffiliateId, int newClientId, int newObjectiveId, int newPortfolioId, decimal newTotalBalance,
        int newTotalUnits, decimal newPrincipal, decimal newEarnings, int newTaxCondition, int newContingentWithholding
    )
    {
        AffiliateId = newAffiliateId;
        ClientId = newClientId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        TotalBalance = newTotalBalance;
        TotalUnits = newTotalUnits;
        Principal = newPrincipal;
        Earnings = newEarnings;
        TaxCondition = newTaxCondition;
        ContingentWithholding = newContingentWithholding;
    }
}