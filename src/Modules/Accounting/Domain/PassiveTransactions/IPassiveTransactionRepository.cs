namespace Accounting.Domain.PassiveTransactions;

public interface IPassiveTransactionRepository
{
    Task<PassiveTransaction?> GetByPortfolioIdAndOperationTypeAsync(int portfolioId, long operationTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PassiveTransaction?>> GetAccountingOperationsAsync(IEnumerable<int> portfolioId, IEnumerable<long> typeOperationsId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PassiveTransaction>> GetByPortfolioIdsAndOperationTypeAsync(IEnumerable<int> portfolioIds, long operationTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PassiveTransaction>> GetByPortfolioIdsAndOperationTypesAsync(IEnumerable<int> portfolioIds, IEnumerable<long> operationTypeIds, CancellationToken cancellationToken = default);

    void Insert(PassiveTransaction passiveTransaction);
    void Update(PassiveTransaction passiveTransaction);
    void Delete(PassiveTransaction passiveTransaction);
}
