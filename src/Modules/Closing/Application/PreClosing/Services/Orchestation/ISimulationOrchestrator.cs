

using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Orchestation
{
    public interface ISimulationOrchestrator
    {
        public Task<bool> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken cancellationToken);
    }
}
