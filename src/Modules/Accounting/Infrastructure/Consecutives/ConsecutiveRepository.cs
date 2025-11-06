using Accounting.Domain.Constants;
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

    public async Task UpdateAsync(Consecutive consecutive, CancellationToken cancellationToken = default)
    {
        dbContext.Consecutives.Update(consecutive);
        await Task.CompletedTask;
    }

    public async Task UpdateIncomeConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default)
    {
        await dbContext.Consecutives
            .Where(c => c.Nature == NatureTypes.Income)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Number, newConsecutiveNumber),
            cancellationToken);
    }

    public async Task UpdateEgressConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default)
    {
        await dbContext.Consecutives
            .Where(c => c.Nature == NatureTypes.Egress)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Number, newConsecutiveNumber),
            cancellationToken);
    }
}
