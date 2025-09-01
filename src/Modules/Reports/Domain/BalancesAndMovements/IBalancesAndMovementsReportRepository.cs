namespace Reports.Domain.BalancesAndMovements
{
    public interface IBalancesAndMovementsReportRepository
    {
        Task<BalancesResponse> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken);
        Task<MovementsResponse> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken);
    }
}
