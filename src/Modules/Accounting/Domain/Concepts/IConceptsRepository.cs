using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Concepts
{
    public interface IConceptsRepository
    {
        Task<IEnumerable<Concept>> GetConceptsByPortfolioIdsAsync(IEnumerable<int> PortfolioIds, CancellationToken CancellationToken);
    }
}
