namespace Reports.Domain.BalancesAndMovements
{
    public class BalancesAndMovementsReportRequest
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int IdentificationId { get; set; }

        public bool IsValid()
        {
            return startDate != default;
        }
    }
}
