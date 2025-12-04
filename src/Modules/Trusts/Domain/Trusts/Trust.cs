using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Trusts.Domain.Trusts;

public sealed class Trust : Entity
{
    private Trust()
    {
    }

    public long TrustId { get; private set; }
    public int AffiliateId { get; private set; }
    public long ClientOperationId { get; private set; }
    public DateTime CreationDate { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal TotalBalance { get; private set; }
    public decimal TotalUnits { get; private set; }
    public decimal Principal { get; private set; }
    public decimal Earnings { get; private set; }
    public int TaxCondition { get; private set; }
    public decimal ContingentWithholding { get; private set; }
    public decimal EarningsWithholding { get; private set; }
    public decimal AvailableAmount { get; private set; }
    public decimal ProtectedBalance { get; private set; }
    public decimal AgileWithdrawalAvailable { get; private set; }
    public LifecycleStatus Status { get; private set; }
    public DateTime? UpdateDate { get; private set; }

    public static Result<Trust> Create(
    int affiliateId, long clientOperationId, DateTime creationDate, int objectiveId, int portfolioId,
    decimal totalBalance, decimal totalUnits, decimal principal, decimal earnings, int taxCondition,
    decimal contingentWithholding, decimal earningsWithholding, decimal availableAmount, decimal protectedBalance,
    decimal agileWithdrawalAvailable, LifecycleStatus status
)
    {
        var trust = new Trust
        {
            TrustId = default,
            AffiliateId = affiliateId,
            ClientOperationId = clientOperationId,
            CreationDate = creationDate,
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            TotalBalance = totalBalance,
            TotalUnits = totalUnits,
            Principal = principal,
            Earnings = earnings,
            TaxCondition = taxCondition,
            ContingentWithholding = contingentWithholding,
            EarningsWithholding = earningsWithholding,
            AvailableAmount = availableAmount,
            ProtectedBalance = protectedBalance,
            AgileWithdrawalAvailable = agileWithdrawalAvailable,
            Status = status
        };

        trust.Raise(new TrustCreatedDomainEvent(trust.TrustId));
        return Result.Success(trust);
    }

    public void UpdateDetails(
        int newAffiliateId, long newClientOperationId, DateTime newCreationDate, int newObjectiveId, int newPortfolioId,
        decimal newTotalBalance, decimal newTotalUnits, decimal newPrincipal, decimal newEarnings, int newTaxCondition,
        decimal newContingentWithholding, decimal newEarningsWithholding, decimal newAvailableAmount,
        decimal newProtectedBalance, decimal newAgileWithdrawalAvailable, LifecycleStatus newStatus
    )
    {
        AffiliateId = newAffiliateId;
        ClientOperationId = newClientOperationId;
        CreationDate = newCreationDate;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        TotalBalance = newTotalBalance;
        TotalUnits = newTotalUnits;
        Principal = newPrincipal;
        Earnings = newEarnings;
        TaxCondition = newTaxCondition;
        ContingentWithholding = newContingentWithholding;
        EarningsWithholding = newEarningsWithholding;
        AvailableAmount = newAvailableAmount;
        ProtectedBalance = newProtectedBalance;
        AgileWithdrawalAvailable = newAgileWithdrawalAvailable;
        Status = newStatus;
    }

    public void UpdateState(
        decimal totalBalance,
        decimal totalUnits,
        decimal principal,
        decimal earnings,
        decimal contingentWithholding,
        decimal earningsWithholding,
        decimal availableAmount,
        decimal protectedBalance,
        decimal agileWithdrawalAvailable,
        LifecycleStatus status,
        DateTime updateDate)
    {
        TotalBalance = totalBalance;
        TotalUnits = totalUnits;
        Principal = principal;
        Earnings = earnings;
        ContingentWithholding = contingentWithholding;
        EarningsWithholding = earningsWithholding;
        AvailableAmount = availableAmount;
        ProtectedBalance = protectedBalance;
        AgileWithdrawalAvailable = agileWithdrawalAvailable;
        Status = status;
        UpdateDate = updateDate;
    }
}
