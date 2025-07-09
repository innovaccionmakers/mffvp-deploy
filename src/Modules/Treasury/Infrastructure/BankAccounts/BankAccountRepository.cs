using Microsoft.EntityFrameworkCore;
using Treasury.Domain.BankAccounts;
using Treasury.Infrastructure.Database;

namespace Treasury.Infrastructure.BankAccounts;

public class BankAccountRepository(TreasuryDbContext context) : IBankAccountRepository
{
    public async Task<BankAccount?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<BankAccount>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts.ToListAsync(cancellationToken);
    }

    public void Add(BankAccount bankAccount)
    {
        context.BankAccounts.Add(bankAccount);
    }
}