using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Integrations.AccountingAccount.GetAccountList
{
    public sealed record GetAccountListQuery : IQuery<IReadOnlyCollection<AccountingAccountResponse>>;
}
