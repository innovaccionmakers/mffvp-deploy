using Closing.Domain.TrustYields;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.TrustYields;

internal sealed class TrustYieldRepository(ClosingDbContext context) : ITrustYieldRepository
{

    public async Task InsertAsync(TrustYield trustYield, CancellationToken cancellationToken = default)
    {
        await context.TrustYields.AddAsync(trustYield, cancellationToken);
    }

    public void Update(TrustYield trustYield)
    {
        context.TrustYields.Update(trustYield);
    }

    public async Task<TrustYield?> GetReadOnlyByTrustAndDateAsync(long trustId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.TrustYields.AsNoTracking().TagWith("TrustYieldRepository_GetReadOnlyByTrustAndDateAsync")
            .SingleOrDefaultAsync(x => x.TrustId == trustId && x.ClosingDate == closingDateUtc, cancellationToken);
    }


    public async Task<IReadOnlyCollection<TrustYield>> GetForUpdateByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken ct)
    {
        return await context.TrustYields
            .TagWith("TrustYieldRepository_GetForUpdateByPortfolioAndDateAsync")
            .Where(t => t.PortfolioId == portfolioId && t.ClosingDate == closingDateUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<TrustYield>> GetReadOnlyByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken ct)
    {
        return await context
            .TrustYields
            .TagWith("TrustYieldRepository_GetReadOnlyByPortfolioAndDateAsync")
            .AsNoTracking() 
            .Where(t => t.PortfolioId == portfolioId && t.ClosingDate == closingDateUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<PortfolioTrustIds>> GetTrustIdsByPortfolioAsync(DateTime closingDate, CancellationToken ct)
    {
        return await context.TrustYields
            .TagWith("TrustYieldRepository_GetTrustIdsByPortfolioAsync")
            .Where(t => t.ClosingDate.Date == closingDate.Date)
            .GroupBy(t => t.PortfolioId)
            .Select(g => new PortfolioTrustIds(
                g.Key,
                g.Select(ty => ty.TrustId).ToList()
            ))
            .ToListAsync(ct);
    }

    public async Task<int> DeleteByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
    {
        var deleted = await context.TrustYields
            .TagWith("TrustYieldRepository_DeleteByPortfolioAndDateAsync")
            .TagWith($"portfolioId={portfolioId} closingDateUtc={closingDateUtc:O}")
            .Where(t => t.PortfolioId == portfolioId && t.ClosingDate == closingDateUtc)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyDictionary<long, TrustYield>> GetReadOnlyByTrustIdsAndDateAsync(IEnumerable<long> trustIds, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        var trustIdsList = trustIds.ToList();
        var results = await context.TrustYields
            .TagWith("TrustYieldRepository_GetReadOnlyByTrustIdsAndDateAsync")
            .AsNoTracking()
            .Where(x => trustIdsList.Contains(x.TrustId) && x.ClosingDate == closingDateUtc)
            .ToListAsync(cancellationToken);

        return results.ToDictionary(x => x.TrustId, x => x);
    }

}

