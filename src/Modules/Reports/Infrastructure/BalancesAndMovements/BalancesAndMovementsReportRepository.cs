using Reports.Domain.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Infrastructure.BalancesAndMovements
{
    internal class BalancesAndMovementsReportRepository(IReportsDbConnectionFactory dbConnectionFactory) : IBalancesAndMovementsReportRepository
    {
        public Task<BalancesResponse> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest)
        {
            throw new NotImplementedException();
        }

        public Task<MovementsResponse> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest)
        {
            throw new NotImplementedException();
        }
    }
}
