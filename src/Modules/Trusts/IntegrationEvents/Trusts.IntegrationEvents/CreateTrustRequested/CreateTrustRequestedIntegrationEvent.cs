using Common.SharedKernel.Application.EventBus;

namespace Trusts.IntegrationEvents.CreateTrustRequested;

public sealed class CreateTrustRequestedIntegrationEvent : IntegrationEvent
{
    public CreateTrustRequestedIntegrationEvent(
        int affiliateId,
        long clientOperationId,
        DateTime creationDate,
        int objectiveId,
        int portfolioId,
        decimal totalBalance,
        int totalUnits,
        decimal principal,
        decimal earnings,
        int taxCondition,
        decimal contingentWithholding,
        decimal earningsWithholding,
        decimal availableAmount)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        AffiliateId = affiliateId;
        ClientOperationId = clientOperationId;
        CreationDate = creationDate;
        ObjectiveId = objectiveId;
        PortfolioId = portfolioId;
        TotalBalance = totalBalance;
        TotalUnits = totalUnits;
        Principal = principal;
        Earnings = earnings;
        TaxCondition = taxCondition;
        ContingentWithholding = contingentWithholding;
        EarningsWithholding = earningsWithholding;
        AvailableAmount = availableAmount;
    }

    public int AffiliateId { get; init; }
    public long ClientOperationId { get; init; }
    public DateTime CreationDate { get; init; }
    public int ObjectiveId { get; init; }
    public int PortfolioId { get; init; }
    public decimal TotalBalance { get; init; }
    public int TotalUnits { get; init; }
    public decimal Principal { get; init; }
    public decimal Earnings { get; init; }
    public int TaxCondition { get; init; }
    public decimal ContingentWithholding { get; init; }
    public decimal EarningsWithholding { get; init; }
    public decimal AvailableAmount { get; init; }
}