using Microsoft.EntityFrameworkCore;
using Security.Domain.Logs;
using Security.Infrastructure.Database;

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
    public void Update(Log log) => context.Set<Log>().Update(log);
    public void Delete(Log log) => context.Set<Log>().Remove(log);
}
