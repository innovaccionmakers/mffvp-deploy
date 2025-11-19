using Microsoft.EntityFrameworkCore;
using Security.Domain.Logs;
using Security.Infrastructure.Database;
using System.Threading;

namespace Security.Infrastructure.Logs;

internal sealed class LogRepository(SecurityDbContext context) : ILogRepository
{
    public async Task<IReadOnlyCollection<Log>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<Log>().ToListAsync(cancellationToken);
    }

    public async Task<Log?> GetAsync(long logId, CancellationToken cancellationToken = default)
    {
        return await context.Set<Log>().SingleOrDefaultAsync(l => l.Id == logId, cancellationToken);
    }

    public void Insert(Log log) => context.Set<Log>().Add(log);

    public async Task Update(Log log, CancellationToken cancellationToken = default)
    {
        await context.Set<Log>()
            .Where(l => l.Id == log.Id)
            .ExecuteUpdateAsync(setters =>
            setters.SetProperty(l => l.SuccessfulProcess, log.SuccessfulProcess),
            cancellationToken);
    }

    public void Delete(Log log) => context.Set<Log>().Remove(log);
}
