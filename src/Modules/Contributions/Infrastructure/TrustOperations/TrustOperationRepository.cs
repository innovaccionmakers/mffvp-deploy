using Microsoft.EntityFrameworkCore;
using Contributions.Domain.TrustOperations;
using Contributions.Infrastructure.Database;

namespace Contributions.Infrastructure;

internal sealed class TrustOperationRepository(ContributionsDbContext context) : ITrustOperationRepository
{
    public async Task<IReadOnlyCollection<TrustOperation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TrustOperations.ToListAsync(cancellationToken);
    }

    public async Task<TrustOperation?> GetAsync(Guid trustoperationId, CancellationToken cancellationToken = default)
    {
        return await context.TrustOperations
            .SingleOrDefaultAsync(x => x.TrustOperationId == trustoperationId, cancellationToken);
    }

    public void Insert(TrustOperation trustoperation)
    {
        context.TrustOperations.Add(trustoperation);
    }

    public void Update(TrustOperation trustoperation)
    {
        context.TrustOperations.Update(trustoperation);
    }

    public void Delete(TrustOperation trustoperation)
    {
        context.TrustOperations.Remove(trustoperation);
    }
}