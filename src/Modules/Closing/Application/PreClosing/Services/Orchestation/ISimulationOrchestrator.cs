using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Orchestation
{
    public interface ISimulationOrchestrator
    {
        public Task<Result<SimulatedYieldResult>> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken cancellationToken);
    }
}
