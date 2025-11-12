using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.AccountingAccounts
{
    public interface IAccountingAccountRepository
    {
        Task<IReadOnlyCollection<AccountingAccount>> GetAccountListAsync(CancellationToken cancellationToken);
    }
}
