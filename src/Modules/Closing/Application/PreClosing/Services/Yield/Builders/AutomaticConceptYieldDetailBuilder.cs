
using Closing.Application.PreClosing.Services.AutomaticConcepts.Dto;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.Time;

namespace Closing.Application.PreClosing.Services.Yield.Builders;

public class AutomaticConceptYieldDetailBuilder : IYieldDetailBuilder
{
    public bool CanHandle(Type type) => type == typeof(AutomaticConceptSummary);

    public YieldDetail Build(object concept, RunSimulationParameters parameters)
    {
        var summary = (AutomaticConceptSummary)concept;
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);
        var nowUtc = DateTime.UtcNow;

        var income = summary.TotalAmount; 
        var expense = 0;

        return YieldDetail.Create(
            parameters.PortfolioId,
            closingDateUtc,
            summary.Source,
            AutomaticConceptSummaryExtensions.ToJsonSummary(summary),
            income,
            expense,
            0m,
            nowUtc,
            parameters.IsClosing
        ).Value;
    }
}