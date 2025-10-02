namespace Accounting.Domain.PassiveTransactions;

public interface IPassiveTransactionRepository
{
    Task<PassiveTransaction?> GetByPortfolioIdAndOperationTypeAsync(int portfolioId, long operationTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PassiveTransaction?>> GetAccountingOperationsAsync(IEnumerable<int> portfolioId, IEnumerable<long> typeOperationsId, CancellationToken cancellationToken = default);
}
