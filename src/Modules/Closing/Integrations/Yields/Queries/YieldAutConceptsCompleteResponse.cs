using Closing.Integrations.YieldDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Closing.Integrations.Yields.Queries
{
    public sealed record class YieldAutConceptsCompleteResponse(
        IReadOnlyCollection<YieldAutConceptsResponse> Yields,
        IReadOnlyCollection<YieldDetailsAutConceptsResponse> YieldDetails
    );
}
