namespace Reports.Domain.BalancesAndMovements
{
    public class BalancesAndMovementsReportRequest
    {
        public DateOnly startDate { get; set; }
        public DateOnly endDate { get; set; }
        public string Identification { get; set; }

        public bool IsValid()
        {
            return startDate != default;
        }
    }
}
