using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Yield;

public class YieldDetailBuilderService
{
    private readonly IEnumerable<IYieldDetailBuilder> _builders;

    public YieldDetailBuilderService(IEnumerable<IYieldDetailBuilder> builders)
    {
        _builders = builders;
    }

    public IReadOnlyList<YieldDetail> Build<T>(IEnumerable<T> concepts, RunSimulationParameters parameters)
    {
        var builder = _builders.FirstOrDefault(b => b.CanHandle(typeof(T)))
            ?? throw new InvalidOperationException($"No builder found for concept type: {typeof(T).Name}");

        return concepts.Select(concept => builder.Build(concept!, parameters)).ToList();
    }
}
