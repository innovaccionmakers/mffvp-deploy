namespace Accounting.Domain.PassiveTransactions;

public interface IPassiveTransactionRepository
{
    Task<PassiveTransaction?> GetByPortfolioIdAsync(int portfolioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PassiveTransaction?>> GetAccountingOperationsAsync(IEnumerable<int> portfolioId, IEnumerable<long> typeOperationsId, CancellationToken cancellationToken = default);
}
