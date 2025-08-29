namespace Reports.Domain.BalancesAndMovements
{
    public interface IBalancesAndMovementsReportRepository
    {
        Task<BalancesResponse> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest);
        Task<MovementsResponse> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest);
    }
}
