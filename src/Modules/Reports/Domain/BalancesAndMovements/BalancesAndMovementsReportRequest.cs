namespace Reports.Domain.BalancesAndMovements
{
    public class BalancesAndMovementsReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Identification { get; set; }

        public bool IsValid()
        {
            return StartDate != default;
        }
    }
}
