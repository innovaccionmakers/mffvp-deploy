namespace MFFVP.BFF.DTOs
{
    public class TechnicalSheetRequestDto
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int PortfolioId { get; set; }
    }
}