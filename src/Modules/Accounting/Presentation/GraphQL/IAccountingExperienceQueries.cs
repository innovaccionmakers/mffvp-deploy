using Accounting.Presentation.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Presentation.GraphQL
{
    public interface IAccountingExperienceQueries
    {
        Task<IReadOnlyCollection<AccountingAccountsDto>> GetAccountListAsync(
          CancellationToken cancellationToken = default);
    }
}
