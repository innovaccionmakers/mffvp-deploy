using Reports.Domain.BalancesAndMovements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Domain.Deposits
{
    public interface IDepositsRepository
    {
        Task<IEnumerable<DepositsResponse>> GetDepositsAsync(DepositsRequest reportRequest, CancellationToken cancellationToken);
    }
}
