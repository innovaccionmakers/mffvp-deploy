using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Core.Primitives;

namespace Closing.IntegrationEvents.DataSync.CreateClientOperationRequested;

public sealed class CreateClientOperationRequestedIntegrationEvent : IntegrationEvent
{
    public CreateClientOperationRequestedIntegrationEvent(
        long clientOperationId,
        DateTime filingDate,
        int affiliateId,
        int objectiveId,
        int portfolioId,
        decimal amount,
        DateTime processDate,
        long transactionSubtypeId,
        DateTime applicationDate,
        LifecycleStatus status,
        int? causeId,
        long? trustId,
        long? linkedClientOperationId,
        decimal? units)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        ClientOperationId = clientOperationId;
        FilingDate = filingDate;
        AffiliateId = affiliateId;
        ObjectiveId = objectiveId;
        PortfolioId = portfolioId;
        Amount = amount;
        ProcessDate = processDate;
        TransactionSubtypeId = transactionSubtypeId;
        ApplicationDate = applicationDate;
        Status = status;
        CauseId = causeId;
        TrustId = trustId;
        LinkedClientOperationId = linkedClientOperationId;
        Units = units;
    }

    public long ClientOperationId { get; init; }
    public DateTime FilingDate { get; init; }
    public int AffiliateId { get; init; }
    public int ObjectiveId { get; init; }
    public int PortfolioId { get; init; }
    public decimal Amount { get; init; }
    public DateTime ProcessDate { get; init; }
    public long TransactionSubtypeId { get; init; }
    public DateTime ApplicationDate { get; init; }
    public LifecycleStatus Status { get; init; }z
    public int? CauseId { get; init; }
    public long? TrustId { get; init; }
    public long? LinkedClientOperationId { get; init; }
    public decimal? Units { get; init; }
}