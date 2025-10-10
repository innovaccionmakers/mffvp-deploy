using Common.SharedKernel.Core.Primitives;

namespace Closing.Integrations.ClientOperations.CreateClientOperation;

public sealed record ClientOperationResponse(
    long ClientOperationId,
    DateTime FilingDate,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    decimal Amount,
    DateTime ProcessDate,
    long TransactionSubtypeId,
    DateTime ApplicationDate,
    LifecycleStatus Status,
    int? CauseId,
    long? TrustId,
    long? LinkedClientOperationId,
    decimal? Units
);