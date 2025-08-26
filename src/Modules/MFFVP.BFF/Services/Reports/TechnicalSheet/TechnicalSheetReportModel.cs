using MFFVP.BFF.Services.Reports.Models;

namespace MFFVP.BFF.Services.Reports.TechnicalSheet;

public sealed class TechnicalSheetReportModel : ReportModelBase
{
    public DateTime Date { get; set; }
    public decimal Contributions {get; set;}
    public decimal Withdrawals {get; set;}
    public decimal GrossPnl {get; set;}
    public decimal Expenses {get; set;}
    public decimal DailyCommission {get; set;}
    public decimal DailyCost {get; set;}
    public decimal CreditedYields {get; set;}
    public decimal GrossUnitYield {get; set;}
    public decimal UnitCost {get; set;}
    public decimal UnitValue {get; set;}
    public decimal Units {get; set;}
    public decimal PortfolioValue {get; set;}
    public int Participants {get; set;}

    public TechnicalSheetReportModel(
        DateTime date,
        decimal contributions,
        decimal withdrawals,
        decimal grossPnl,
        decimal expenses,
        decimal dailyCommission,
        decimal dailyCost,
        decimal creditedYields,
        decimal grossUnitYield,
        decimal unitCost,
        decimal unitValue,
        decimal units,
        decimal portfolioValue,
        int participants
        )
    {
        Date = date;
        Contributions = contributions;
        Withdrawals = withdrawals;
        GrossPnl = grossPnl;
        Expenses = expenses;
        DailyCommission = dailyCommission;
        DailyCost = dailyCost;
        CreditedYields = creditedYields;
        GrossUnitYield = grossUnitYield;    
        UnitCost = unitCost;
        UnitValue = unitValue;
        Units = units;
        PortfolioValue = portfolioValue;
        Participants = participants;
    }
    public override object[] ToRowData()
    {
        return new object[]
        {
            Date.ToString("yyyyMMdd"),
            Contributions,
            Withdrawals,
            GrossPnl,
            Expenses,
            DailyCommission,
            DailyCost,
            CreditedYields,
            GrossUnitYield,
            UnitCost,
            UnitValue,
            Units,
            PortfolioValue,
            Participants
        };
    }
}
