namespace Accounting.Domain.PassiveTransactions;

public interface IPassiveTransactionRepository
{
    Task<PassiveTransaction?> GetByPortfolioIdAsync(int portfolioId, CancellationToken cancellationToken = default);
}
