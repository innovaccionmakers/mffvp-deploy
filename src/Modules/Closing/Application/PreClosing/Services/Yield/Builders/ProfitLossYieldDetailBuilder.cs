﻿using Closing.Application.PreClosing.Services.ProfitAndLoss.Dto;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ProfitLosses;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Yield.Builders;

public class ProfitLossYieldDetailBuilder : IYieldDetailBuilder
{
    public bool CanHandle(Type type) => type == typeof(ProfitLossConceptSummary);

    public YieldDetail Build(object concept, RunSimulationParameters parameters)
    {
        var summary = (ProfitLossConceptSummary)concept;
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);

        var income = summary.Nature == IncomeExpenseNature.Income ? summary.TotalAmount : 0;
        var expense = summary.Nature == IncomeExpenseNature.Expense ? summary.TotalAmount : 0;

        return YieldDetail.Create(
            parameters.PortfolioId,
            closingDateUtc,
            summary.Source,
            ProfitLossConceptSummaryExtensions.ToJsonSummary(summary),
            income,
            expense,
            0,
            DateTime.UtcNow,
            parameters.IsClosing
        ).Value;
    }
}
