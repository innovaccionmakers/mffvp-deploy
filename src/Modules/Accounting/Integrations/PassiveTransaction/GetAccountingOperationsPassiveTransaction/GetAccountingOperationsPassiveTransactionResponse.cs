using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction
{
    public sealed record class GetAccountingOperationsPassiveTransactionResponse(
        int PortfolioId,
        string? CreditAccount
        );
}
