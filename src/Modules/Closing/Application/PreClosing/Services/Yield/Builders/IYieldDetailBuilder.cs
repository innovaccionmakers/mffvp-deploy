using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Yield.Builders;

public interface IYieldDetailBuilder
{
    bool CanHandle(Type type);
    YieldDetail Build(object concept, RunSimulationParameters parameters);
}
