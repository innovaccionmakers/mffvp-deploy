using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.ClientOperations.CreateClientOperation;

public sealed record CreateClientOperationCommand(
    long ClientOperationId,
    DateTime FilingDate,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    decimal Amount,
    DateTime ProcessDate,
    long TransactionSubtypeId,
    DateTime ApplicationDate,
    int Status,
    long? TrustId,
    long? LinkedClientOperationId,
    decimal? Units
) : ICommand<ClientOperationResponse>;