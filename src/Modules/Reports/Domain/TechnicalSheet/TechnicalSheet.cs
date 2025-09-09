namespace Reports.Domain.TechnicalSheet;

public sealed class TechnicalSheet
{
    public DateTime Date { get; set; }
    public decimal Contributions { get; set; }
    public decimal Withdrawals { get; set; }
    public decimal GrossPnl { get; set; }
    public decimal Expenses { get; set; }
    public decimal DailyCommission { get; set; }
    public decimal DailyCost { get; set; }
    public decimal CreditedYields { get; set; }
    public decimal GrossUnitYield { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitValue { get; set; }
    public decimal Units { get; set; }
    public decimal PortfolioValue { get; set; }
    public int Participants { get; set; }

    public TechnicalSheet()
    {
    }
}
