using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;

namespace Products.Domain.TechnicalSheets;

public sealed class TechnicalSheet : Entity
{
    public int TechnicalSheetId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Contributions { get; private set; }
    public decimal Withdrawals { get; private set; }
    public decimal GrossPnl { get; private set; }
    public decimal Expenses { get; private set; }
    public decimal DailyCommission { get; private set; }
    public decimal DailyCost { get; private set; }
    public decimal CreditedYields { get; private set; }
    public decimal GrossUnitYield { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal UnitValue { get; private set; }
    public decimal Units { get; private set; }
    public decimal PortfolioValue { get; private set; }
    public int Participants { get; private set; }

    private TechnicalSheet()
    {
    }

    public static Result<TechnicalSheet> Create(
        int portfolioId,
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
        var technicalSheet = new TechnicalSheet
        {
            TechnicalSheetId = default,
            PortfolioId = portfolioId,
            Date = date,
            Contributions = contributions,
            Withdrawals = withdrawals,
            GrossPnl = grossPnl,
            Expenses = expenses,
            DailyCommission = dailyCommission,
            DailyCost = dailyCost,
            CreditedYields = creditedYields,
            GrossUnitYield = grossUnitYield,
            UnitCost = unitCost,
            UnitValue = unitValue,
            Units = units,
            PortfolioValue = portfolioValue,
            Participants = participants
        };

        return Result.Success(technicalSheet);
    }

    public void UpdateDetails(
        DateTime newDate,
        decimal newContributions,
        decimal newWithdrawals,
        decimal newGrossPnl,
        decimal newExpenses,
        decimal newDailyCommission,
        decimal newDailyCost,
        decimal newCreditedYields,
        decimal newGrossUnitYield,
        decimal newUnitCost,
        decimal newUnitValue,
        decimal newUnits,
        decimal newPortfolioValue,
        int newParticipants
    )
    {
        Date = newDate;
        Contributions = newContributions;
        Withdrawals = newWithdrawals;
        GrossPnl = newGrossPnl;
        Expenses = newExpenses;
        DailyCommission = newDailyCommission;
        DailyCost = newDailyCost;
        CreditedYields = newCreditedYields;
        GrossUnitYield = newGrossUnitYield;
        UnitCost = newUnitCost;
        UnitValue = newUnitValue;
        Units = newUnits;
        PortfolioValue = newPortfolioValue;
        Participants = newParticipants;
    }
}
