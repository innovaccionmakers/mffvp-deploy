using Microsoft.EntityFrameworkCore;
using Products.Domain.Objectives;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Objectives;

internal sealed class ObjectiveRepository(ProductsDbContext context) : IObjectiveRepository
{
    public async Task<IReadOnlyCollection<Objective>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Objectives.ToListAsync(cancellationToken);
    }

    public async Task<Objective?> GetAsync(int objectiveId, CancellationToken cancellationToken = default)
    {
        return await context.Objectives
            .SingleOrDefaultAsync(x => x.ObjectiveId == objectiveId, cancellationToken);
    }

    public async Task<Objective?> GetByIdAsync(int objectiveId, CancellationToken ct)
    {
        return await context.Objectives
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ObjectiveId == objectiveId, ct);
    }
    
    public async Task<IReadOnlyCollection<Objective>> GetByAffiliateAsync(
        int affiliateId, CancellationToken cancellationToken = default)
    {
        return await context.Objectives
            .AsNoTracking()
            .Include(o => o.Alternative) 
            .Where(o => o.AffiliateId == affiliateId)
            .ToListAsync(cancellationToken);
    }
    
    public Task<bool> AnyAsync(int affiliateId, CancellationToken ct = default) =>
        context.Objectives
            .AsNoTracking()
            .Where(o => o.AffiliateId == affiliateId)
            .AnyAsync(ct);

    public Task<bool> AnyWithStatusAsync(int affiliateId, string status, CancellationToken ct = default) =>
        context.Objectives
            .AsNoTracking()
            .Where(o => o.AffiliateId == affiliateId && o.Status == status)
            .AnyAsync(ct);

    public async Task<IReadOnlyCollection<Objective>> GetByAffiliateAsync(
        int affiliateId,
        string? status,
        CancellationToken ct = default)
    {
        var query = context.Objectives
            .Include(o => o.Alternative)
            .AsNoTracking()
            .Where(o => o.AffiliateId == affiliateId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        return await query.ToListAsync(ct);
    }

}