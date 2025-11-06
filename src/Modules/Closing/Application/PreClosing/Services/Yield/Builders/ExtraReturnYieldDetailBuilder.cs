using Closing.Application.PreClosing.Services.ExtraReturns.Dto;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;

using Common.SharedKernel.Application.Helpers.Time;

namespace Closing.Application.PreClosing.Services.Yield.Builders;

public sealed class ExtraReturnYieldDetailBuilder : IYieldDetailBuilder
{
    public bool CanHandle(Type type) => type == typeof(ExtraReturnSummary);

    public YieldDetail Build(object concept, RunSimulationParameters parameters)
    {
        var summary = (ExtraReturnSummary)concept;
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);

        return YieldDetail.Create(
            parameters.PortfolioId,
            closingDateUtc,
            YieldsSources.ExtraReturn,
            summary.ToJsonSummary(),
            -summary.Amount,
            0,
            0,
            summary.ProcessDateUtc,
            parameters.IsClosing
        ).Value;
    }
}
