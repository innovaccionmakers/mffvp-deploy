using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Integrations.Treasuries.GetTreasuriesByPortfolioIds
{
    public sealed record class GetTreasuriesByPortfolioIdsResponse(
        int PortfolioIds,
        string? DebitAccount
        );
}
