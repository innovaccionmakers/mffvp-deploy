using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<decimal> GetDistributedTotalRoundedAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        return await context.TrustYields
            .Where(t => t.PortfolioId == portfolioId && t.ClosingDate == closingDate)
            .Select(t => Math.Round(t.YieldAmount, 2)) 
            .TagWith("TrustYieldRepository_GetDistributedTotalRoundedAsync")
            .SumAsync(cancellationToken);

    }

    public async Task<IReadOnlyList<TrustYieldCalcInput>> GetCalcInputsByPortfolioAndDateAsync(
      int portfolioId,
      DateTime closingDateUtc,
      CancellationToken cancellationToken = default)
    {
        var previousDateUtc = closingDateUtc.AddDays(-1);

        // Yields del día actual
        var currentDay = context.TrustYields
            .TagWith("TrustYieldRepository_GetCalcInputsByPortfolioAndDateAsync_Current")
            .AsNoTracking()
            .Where(current =>
                current.PortfolioId == portfolioId &&
                current.ClosingDate == closingDateUtc);

        // Yields del día anterior
        var prevDay = context.TrustYields
            .TagWith("TrustYieldRepository_GetCalcInputsByPortfolioAndDateAsync_Previous")
            .AsNoTracking()
            .Where(prev =>
                prev.PortfolioId == portfolioId &&
                prev.ClosingDate == previousDateUtc);

        // Left join por (PortfolioId, TrustId)
        var query =
           from current in currentDay
           join prev in prevDay
               on new { current.PortfolioId, current.TrustId }
               equals new { prev.PortfolioId, prev.TrustId }
               into prevGroup
           from prev in prevGroup.DefaultIfEmpty()
           select new TrustYieldCalcInput(
               current.TrustId,
               current.PortfolioId,
               current.PreClosingBalance, //Saldo pre cierre del día actual
               current.Units, //Unidades del día actual
               prev == null, //isFirstTrustClosingDay: Es el primer dia de cierre del fideicomiso si no hay registro previo
               prev != null ? prev.ClosingBalance : 0,
               prev != null ? prev.Units :0 
       );
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteByIdsAsync(IEnumerable<long> trustYieldIds, CancellationToken cancellationToken = default)
    {
        var ids = trustYieldIds?.Distinct().ToArray() ?? Array.Empty<long>();
        if (ids.Length == 0)
        {
            return 0;
        }

        return await context.TrustYields
            .TagWith("TrustYieldRepository_DeleteByIdsAsync")
            .Where(t => ids.Contains(t.TrustYieldId))
            .ExecuteDeleteAsync(cancellationToken);
    }

}

