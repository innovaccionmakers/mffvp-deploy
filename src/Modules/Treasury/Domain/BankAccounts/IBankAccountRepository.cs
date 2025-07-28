using Treasury.Domain.BankAccounts;

namespace Treasury.Domain.BankAccounts;

public interface IBankAccountRepository
{
    Task<BankAccount?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BankAccount>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(BankAccount bankAccount, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long issuerId, string accountNumber, string accountType, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BankAccount>> GetByPortfolioIdAsync(long portfolioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BankAccount>> GetByPortfolioAndIssuerAsync(long portfolioId, long issuerId, CancellationToken cancellationToken = default);
}