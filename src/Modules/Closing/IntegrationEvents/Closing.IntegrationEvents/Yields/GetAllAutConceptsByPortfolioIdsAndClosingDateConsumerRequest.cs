using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Closing.IntegrationEvents.Yields
{
    public sealed record class GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest(IEnumerable<int> PortfolioIds, DateTime ClosingDate);
}
