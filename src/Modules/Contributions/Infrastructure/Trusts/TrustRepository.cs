using Microsoft.EntityFrameworkCore;
using Contributions.Domain.Trusts;
using Contributions.Infrastructure.Database;

namespace Contributions.Infrastructure;

internal sealed class TrustRepository(ContributionsDbContext context) : ITrustRepository
{
    public async Task<IReadOnlyCollection<Trust>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Trusts.ToListAsync(cancellationToken);
    }

    public async Task<Trust?> GetAsync(Guid trustId, CancellationToken cancellationToken = default)
    {
        return await context.Trusts
            .SingleOrDefaultAsync(x => x.TrustId == trustId, cancellationToken);
    }

    public void Insert(Trust trust)
    {
        context.Trusts.Add(trust);
    }

    public void Update(Trust trust)
    {
        context.Trusts.Update(trust);
    }

    public void Delete(Trust trust)
    {
        context.Trusts.Remove(trust);
    }
}