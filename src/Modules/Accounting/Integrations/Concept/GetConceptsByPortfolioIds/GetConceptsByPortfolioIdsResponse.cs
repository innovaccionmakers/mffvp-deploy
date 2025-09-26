using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Integrations.Treasuries.GetConceptsByPortfolioIds
{
    public sealed record class GetConceptsByPortfolioIdsResponse(
        int PortfolioId,
        string? ContraCreditAccount,
        string? ContraDebitAccount,
        string? DebitAccount,
        string? CreditAccount
        );
}
