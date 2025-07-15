using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Yield;

public interface IYieldPersistenceService
{
    Task<SimulatedYieldResult> ConsolidateAsync(
        int portfolioId,
        DateTime closingDateLocal,
        bool isClosed,
        CancellationToken ct = default);
}
