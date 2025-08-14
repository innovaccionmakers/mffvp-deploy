using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Integrations.ClientOperations.GetClientOperationsByProcessDate
{
    public sealed record class GetClientOperationsByProcessDateQuery(DateTime ProcessDate) : IQuery<IEnumerable<GetClientOperationsByProcessDateResponse>>;
}
