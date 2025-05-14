using Microsoft.EntityFrameworkCore;
using Trusts.Domain.TrustHistories;
using Trusts.Infrastructure.Database;

namespace Trusts.Infrastructure;

internal sealed class TrustHistoryRepository(TrustsDbContext context) : ITrustHistoryRepository
{
    public async Task<IReadOnlyCollection<TrustHistory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TrustHistories.ToListAsync(cancellationToken);
    }

    public async Task<TrustHistory?> GetAsync(long trusthistoryId, CancellationToken cancellationToken = default)
    {
        return await context.TrustHistories
            .SingleOrDefaultAsync(x => x.TrustHistoryId == trusthistoryId, cancellationToken);
    }

    public void Insert(TrustHistory trusthistory)
    {
        context.TrustHistories.Add(trusthistory);
    }

    public void Update(TrustHistory trusthistory)
    {
        context.TrustHistories.Update(trusthistory);
    }

    public void Delete(TrustHistory trusthistory)
    {
        context.TrustHistories.Remove(trusthistory);
    }
}