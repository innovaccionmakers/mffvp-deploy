using Common.SharedKernel.Domain;

namespace Reports.Domain.BalancesAndMovements
{
    public interface IBalancesAndMovementsReportRepository
    {
        Task<IEnumerable<BalancesResponse>> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken);
        Task<IEnumerable<MovementsResponse>> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken);
    }
}
