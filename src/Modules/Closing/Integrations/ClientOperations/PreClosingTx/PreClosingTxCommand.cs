using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;

namespace Closing.Integrations.ClientOperations.PreClosingTx;

public sealed record PreClosingTxCommand(
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
) : ICommand<PreClosingTxResponse>;
