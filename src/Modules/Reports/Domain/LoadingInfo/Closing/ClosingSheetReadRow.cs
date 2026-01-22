namespace Reports.Domain.LoadingInfo.Closing;

public sealed class ClosingSheetReadRow
{
    public int PortfolioId { get; set; }
    public DateTime ClosingDate { get; set; }
    public decimal Contributions { get; set; }
    public decimal Withdrawals { get; set; }
    public decimal GrossPandL { get; set; }
    public decimal Expenses { get; set; }
    public decimal DailyFee { get; set; }
    public decimal DailyCost { get; set; }
    public decimal YieldsToCredit { get; set; }
    public decimal GrossYieldPerUnit { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal UnitValue { get; set; }
    public decimal Units { get; set; }
 public decimal PortfolioValue { get; set; }
    public long Participants { get; set; }
    public decimal? PortfolioFeePercentage { get; set; }
    public int? FundId { get; set; }
}
