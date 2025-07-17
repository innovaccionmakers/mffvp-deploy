using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Yield.Interfaces;

public interface IYieldDetailBuilder
{
    bool CanHandle(Type type);
    YieldDetail Build(object concept, RunSimulationParameters parameters);
}
