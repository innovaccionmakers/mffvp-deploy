using Common.SharedKernel.Domain;

namespace Reports.Domain.LoadingInfo.Closing;

public sealed class ClosingSheet : Entity
{
    public long Id { get; private set; }

    public int PortfolioId { get; private set; }
    public DateTime ClosingDate { get; private set; }

    public decimal Contributions { get; private set; }
    public decimal Withdrawals { get; private set; }

    public decimal GrossPandL { get; private set; }
    public decimal Expenses { get; private set; }
    public decimal DailyFee { get; private set; }
    public decimal DailyCost { get; private set; }

    public decimal YieldsToCredit { get; private set; }

    public decimal GrossYieldPerUnit { get; private set; }
    public decimal CostPerUnit { get; private set; }
    public decimal UnitValue { get; private set; }
    public decimal Units { get; private set; }
    public decimal PortfolioValue { get; private set; }

    public int Participants { get; private set; }

    public decimal PortfolioFeePercentage { get; private set; }
    public int? FundId { get; private set; }

    public static Result<ClosingSheet> Create(ClosingSheetReadRow row)
    {
        var entity = new ClosingSheet
        {
            Id = default,
            PortfolioId = row.PortfolioId,
            ClosingDate = row.ClosingDate,

            Contributions = row.Contributions,
            Withdrawals = row.Withdrawals,

            GrossPandL = row.GrossPandL,
            Expenses = row.Expenses,
            DailyFee = row.DailyFee,
            DailyCost = row.DailyCost,

            YieldsToCredit = row.YieldsToCredit,

            GrossYieldPerUnit = row.GrossYieldPerUnit,
            CostPerUnit = row.CostPerUnit,
            UnitValue = row.UnitValue,
            Units = row.Units,
            PortfolioValue = row.PortfolioValue,

            Participants = (int)row.Participants,
            PortfolioFeePercentage = (decimal)row.PortfolioFeePercentage,
            FundId = row.FundId
        };

        return Result.Success(entity);
    }
}