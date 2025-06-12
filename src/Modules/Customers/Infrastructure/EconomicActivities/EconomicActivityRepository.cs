using Microsoft.EntityFrameworkCore;
using Customers.Domain.EconomicActivities;
using Customers.Infrastructure.Database;

namespace Customers.Infrastructure.EconomicActivities;

internal sealed class EconomicActivityRepository(CustomersDbContext context) : IEconomicActivityRepository
{
    public async Task<IReadOnlyCollection<EconomicActivity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.EconomicActivities.ToListAsync(cancellationToken);
    }

    public async Task<EconomicActivity?> GetAsync(int economicactivityId,
        CancellationToken cancellationToken = default)
    {
        return await context.EconomicActivities
            .SingleOrDefaultAsync(x => x.EconomicActivityId == economicactivityId, cancellationToken);
    }

    public void Insert(EconomicActivity economicactivity)
    {
        context.EconomicActivities.Add(economicactivity);
    }

    public void Update(EconomicActivity economicactivity)
    {
        context.EconomicActivities.Update(economicactivity);
    }

    public void Delete(EconomicActivity economicactivity)
    {
        context.EconomicActivities.Remove(economicactivity);
    }
}