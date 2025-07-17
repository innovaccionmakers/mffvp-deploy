using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.Constants;
using Closing.Domain.TreasuryMovements;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Yield.Builders;

public class TreasuryYieldDetailBuilder : IYieldDetailBuilder
{
    public bool CanHandle(Type type) => type == typeof(TreasuryMovementSummary);

    public YieldDetail Build(object concept, RunSimulationParameters parameters)
    {
        var summary = (TreasuryMovementSummary)concept;
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);

        var isIncome = summary.Nature == IncomeExpenseNature.Income.ToString();
        var isExpense = summary.Nature == IncomeExpenseNature.Expense.ToString();
        var allowsExpense = summary.AllowsExpense;

        var incomeAmount = isIncome && !allowsExpense
            ? summary.TotalAmount
            : 0;

        var expenseAmount = isExpense
            ? summary.TotalAmount
            : (isIncome && allowsExpense ? -summary.TotalAmount : 0);

        return YieldDetail.Create(
            parameters.PortfolioId,
            closingDateUtc,
            YieldsSources.Treasury,
            TreasuryMovementSummaryExtensions.ToJsonSummary(summary),
            incomeAmount,
            expenseAmount,
            0,
            DateTime.UtcNow,
            parameters.IsClosing
        ).Value;
    }
}
