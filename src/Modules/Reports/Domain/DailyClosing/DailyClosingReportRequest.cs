namespace Reports.Domain.DailyClosing
{
    public class DailyClosingReportRequest
    {
        public int PortfolioId { get; set; }
        public DateTime GenerationDate { get; set; }

        public bool IsValid()
            => PortfolioId > 0 && GenerationDate != default;
    }
}
