namespace Closing.Domain.ClientOperations;

public interface IClientOperationRepository
{
    void Insert(ClientOperation clientOperation);

    Task<bool> ClientOperationsExistsAsync(int portfolioId, DateTime closingDateUtc, long transactionSubtypeId, CancellationToken cancellationToken = default);
} 