using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Yield.Interfaces;

public interface IYieldPersistenceService
{
    Task<SimulatedYieldResult> ConsolidateAsync(RunSimulationParameters parameters, CancellationToken ct = default);
}