namespace Reports.Domain.TechnicalSheet;

public class TechnicalSheetReportRequest
{
    public int PortfolioId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsValid()
        => PortfolioId > 0 && StartDate != default && EndDate != default && StartDate <= EndDate;
}
