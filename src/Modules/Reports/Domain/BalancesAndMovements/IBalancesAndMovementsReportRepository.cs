using Common.SharedKernel.Domain;

namespace Reports.Domain.BalancesAndMovements
{
    public interface IBalancesAndMovementsReportRepository
    {
        Task<IEnumerable<Result<BalancesResponse>>> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken);
        Task<IEnumerable<Result<MovementsResponse>>> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken);
    }
}
