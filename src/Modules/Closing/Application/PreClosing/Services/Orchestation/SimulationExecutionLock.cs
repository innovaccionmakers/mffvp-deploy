
using System.Collections.Concurrent;

namespace Closing.Application.PreClosing.Services.Orchestation;

public sealed class SimulationExecutionLock : ISimulationExecutionLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private static string KeyOf(int portfolioId, DateTime closingDate, bool isClosing) =>
        $"{portfolioId:000000}-{closingDate:yyyyMMdd}-{(isClosing ? "C" : "S")}";

    public async Task<IDisposable> AcquireAsync(int portfolioId, DateTime closingDate, bool isClosing, CancellationToken ct)
    {
        var key = KeyOf(portfolioId, closingDate, isClosing);
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        return new Releaser(sem, key, _locks);
    }

    public Task<IDisposable?> TryAcquireAsync(int portfolioId, DateTime closingDate, bool isClosing, CancellationToken ct)
    {
        var key = KeyOf(portfolioId, closingDate, isClosing);
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        // intento inmediato, no bloqueante
        var acquired = sem.Wait(0);
        return Task.FromResult<IDisposable?>(acquired ? new Releaser(sem, key, _locks) : null);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim sem;
        private readonly string key;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> dict;
        private int disposed; // 0=false, 1=true

        public Releaser(SemaphoreSlim sem, string key, ConcurrentDictionary<string, SemaphoreSlim> dict)
        { this.sem = sem; this.key = key; this.dict = dict; }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 1) return;
            sem.Release();
            if (sem.CurrentCount == 1) dict.TryRemove(key, out _); // limpieza best‑effort
        }
    }
}