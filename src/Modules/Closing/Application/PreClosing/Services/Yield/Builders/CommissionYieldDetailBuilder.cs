using Closing.Application.PreClosing.Services.Commission.Dto;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.General;

namespace Closing.Application.PreClosing.Services.Yield.Builders;

public class CommissionYieldDetailBuilder : IYieldDetailBuilder
{
    public bool CanHandle(Type type) => type == typeof(CommissionConceptSummary);

    public YieldDetail Build(object concept, RunSimulationParameters parameters)
    {
        var summary = (CommissionConceptSummary)concept;
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);

        return YieldDetail.Create(
            parameters.PortfolioId,
            closingDateUtc,
            YieldsSources.Commission,
            CommissionConceptSummaryExtensions.ToJsonSummary(summary),
            0,
            0,
            summary.Value,
            DateTime.UtcNow,
            parameters.IsClosing
        ).Value;
    }
}
