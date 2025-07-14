using Closing.Domain.Constants;
using Closing.Domain.TreasuryMovements;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain.Utils;

namespace Closing.Application.PreClosing.Services.Yield.Builders;

public class TreasuryYieldDetailBuilder : IYieldDetailBuilder
{
    public bool CanHandle(Type type) => type == typeof(TreasuryMovementSummary);

    public YieldDetail Build(object concept, RunSimulationCommand parameters)
    {
        var summary = (TreasuryMovementSummary)concept;
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);

        var isIncome = summary.NatureEnum == TreasuryConceptNature.Income;
        var isExpense = summary.NatureEnum == TreasuryConceptNature.Expense;
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
