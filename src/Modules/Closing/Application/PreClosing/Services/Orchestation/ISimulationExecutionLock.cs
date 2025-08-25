namespace Closing.Application.PreClosing.Services.Orchestation;

public interface ISimulationExecutionLock
{
    Task<IDisposable> AcquireAsync(int portfolioId, DateTime closingDate, bool isClosing, CancellationToken ct);
    Task<IDisposable?> TryAcquireAsync(int portfolioId, DateTime closingDate, bool isClosing, CancellationToken ct);
}