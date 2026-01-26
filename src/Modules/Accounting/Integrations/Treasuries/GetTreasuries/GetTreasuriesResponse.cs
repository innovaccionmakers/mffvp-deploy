using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Integrations.Treasuries.GetTreasuries
{
    public sealed record class GetTreasuriesResponse(
        long TreasuryId,
        string? BankAccount,
        string? DebitAccount,
        string? CreditAccount
        );
}
