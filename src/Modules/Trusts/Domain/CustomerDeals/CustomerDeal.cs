using Common.SharedKernel.Domain;

namespace Trusts.Domain.CustomerDeals;

public sealed class CustomerDeal : Entity
{
    private CustomerDeal()
    {
    }

    public Guid CustomerDealId { get; private set; }
    public DateTime Date { get; private set; }
    public int AffiliateId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public int ConfigurationParamId { get; private set; }
    public decimal Amount { get; private set; }

    public static Result<CustomerDeal> Create(
        DateTime date, int affiliateId, int objectiveId, int portfolioId, int configurationParamId, decimal amount
    )
    {
        var customerdeal = new CustomerDeal
        {
            CustomerDealId = Guid.NewGuid(),
            Date = date,
            AffiliateId = affiliateId,
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            ConfigurationParamId = configurationParamId,
            Amount = amount
        };
        customerdeal.Raise(new CustomerDealCreatedDomainEvent(customerdeal.CustomerDealId));
        return Result.Success(customerdeal);
    }

    public void UpdateDetails(
        DateTime newDate, int newAffiliateId, int newObjectiveId, int newPortfolioId, int newConfigurationParamId,
        decimal newAmount
    )
    {
        Date = newDate;
        AffiliateId = newAffiliateId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        ConfigurationParamId = newConfigurationParamId;
        Amount = newAmount;
    }
}