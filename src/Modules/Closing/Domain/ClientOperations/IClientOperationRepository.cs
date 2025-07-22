namespace Closing.Domain.ClientOperations;

public interface IClientOperationRepository
{
    void Insert(ClientOperation clientOperation);

    Task<bool> ClientOperationsExistsAsync(int portfolioId, DateTime closingDateUtc, long transactionSubtypeId, CancellationToken cancellationToken = default);
    Task<decimal> SumByPortfolioAndSubtypesAsync(
       int portfolioId,
       DateTime closingDateUtc,
       IEnumerable<long> subtransactionTypeIds,
       CancellationToken cancellationToken = default);
} 