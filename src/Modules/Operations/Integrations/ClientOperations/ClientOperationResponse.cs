using Common.SharedKernel.Core.Primitives;

namespace Operations.Integrations.ClientOperations;

public sealed record ClientOperationResponse(
    long ClientOperationId,
    DateTime Date,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    decimal Amount,
    int OperationTypeId,
    LifecycleStatus Status,
    int? CauseId,
    long? TrustId,
    long? LinkedClientOperationId,
    decimal? Units
);