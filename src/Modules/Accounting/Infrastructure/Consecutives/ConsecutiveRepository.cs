using Accounting.Domain.Consecutives;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.Consecutives;

public class ConsecutiveRepository(AccountingDbContext dbContext) : IConsecutiveRepository
{
    public async Task<IReadOnlyCollection<Consecutive>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Consecutives.ToListAsync(cancellationToken);
    }

    public async Task<Consecutive?> GetConsecutiveByNatureAsync(string nature)
    {
        return await dbContext.Consecutives
            .Where(x => x.Nature == nature)
            .FirstOrDefaultAsync();
    }
}
