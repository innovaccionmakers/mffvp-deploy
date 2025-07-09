using Treasury.Domain.BankAccounts;

namespace Treasury.Domain.BankAccounts;

public interface IBankAccountRepository
{
    Task<BankAccount?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BankAccount>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(BankAccount bankAccount);
}