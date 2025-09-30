namespace Accounting.Domain.PassiveTransactions;

public interface IPassiveTransactionRepository
{
    Task<PassiveTransaction?> GetByPortfolioIdAndOperationTypeAsync(int portfolioId, long operationTypeId, CancellationToken cancellationToken = default);
}
