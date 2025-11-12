using Accounting.Domain.AccountingAccounts;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.AccountingAccounts
{
    public class AccountingAccountRepository : IAccountingAccountRepository
    {
        private readonly AccountingDbContext _context;
        public AccountingAccountRepository(AccountingDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<AccountingAccount>> GetAccountListAsync(CancellationToken cancellationToken)
        {
            return await _context.AccountingAccounts.ToListAsync();
        }
    }
}
