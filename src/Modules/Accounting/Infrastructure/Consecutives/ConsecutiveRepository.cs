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

    public async Task UpdateConsecutivesByNatureAsync(Dictionary<string, int> consecutiveNumbersByNature, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in consecutiveNumbersByNature)
        {
            var nature = kvp.Key;
            var newConsecutiveNumber = kvp.Value;

            var consecutive = await GetConsecutiveByNatureAsync(nature);
            if (consecutive != null)
            {
                consecutive.UpdateDetails(
                    consecutive.Nature,
                    consecutive.SourceDocument,
                    newConsecutiveNumber);

                dbContext.Consecutives.Update(consecutive);
            }
        }

        await Task.CompletedTask;
    }
}
