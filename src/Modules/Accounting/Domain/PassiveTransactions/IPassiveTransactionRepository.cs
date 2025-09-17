namespace Accounting.Domain.PassiveTransactions;

public interface IPassiveTransactionRepository
{
    Task<IReadOnlyCollection<PassiveTransaction>> GetByPortfolioIdAsync(int portfolioId);
}
