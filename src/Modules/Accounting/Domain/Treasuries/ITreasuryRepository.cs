using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Treasuries
{
    public interface ITreasuryRepository
    {
        Task<IEnumerable<Treasury>> GetAccountingConceptsTreasuriesAsync(IEnumerable<int> PortfolioIds, CancellationToken CancellationToken);
        Task<IEnumerable<Treasury>> GetAccountingOperationsTreasuriesAsync(IEnumerable<int> PortfolioIds, IEnumerable<string> CollectionAccount, CancellationToken CancellationToken);
    }
}
