using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.YieldDetails;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Closing.Infrastructure.YieldDetails
{
    internal sealed class YieldDetailRepository(ClosingDbContext context, IDbContextFactory<ClosingDbContext> dbFactory) : IYieldDetailRepository
    {

        public async Task InsertAsync(YieldDetail yieldDetail, CancellationToken ct = default)
        {
            await context.YieldDetails.AddAsync(yieldDetail, ct);
        }

        public async Task DeleteByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            var deletedCount = await db.YieldDetails
               .TagWith($"YieldDetailRepository_DeleteByPortfolioAndDateAsync({portfolioId}, {closingDateUtc:yyyy-MM-dd})")
               .Where(yield => yield.PortfolioId == portfolioId
                             && yield.ClosingDate == closingDateUtc
                             && !yield.IsClosed //Solo se pueden borrar los que no son Cerrados
                            &&  yield.Source != YieldsSources.AutomaticConcept) // Excluir los conceptos automáticos calculados el dia anterior
                .ExecuteDeleteAsync(cancellationToken);

        }

        public async Task DeleteClosedByPortfolioAndDateAsync(
            int portfolioId,
            DateTime closingDateUtc,
            CancellationToken cancellationToken = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            await db.YieldDetails
                .TagWith($"YieldDetailRepository_DeleteClosedByPortfolioAndDateAsync({portfolioId}, {closingDateUtc:yyyy-MM-dd})")
                .Where(yield => yield.PortfolioId == portfolioId
                                && yield.ClosingDate == closingDateUtc
                                && yield.IsClosed
                                && yield.Source != YieldsSources.AutomaticConcept) // Excluir los conceptos automáticos calculados el dia anterior

                .ExecuteDeleteAsync(cancellationToken);

        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetReadOnlyByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        bool isClosed = false,
        CancellationToken cancellationToken = default)
        {
            var baseQuery = context.YieldDetails
            .TagWith($"YieldDetailRepository_GetReadOnlyByPortfolioAndDateAsync({portfolioId}, {closingDateUtc:yyyy-MM-dd})")
            .AsNoTracking()
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate == closingDateUtc);

            if (isClosed)
            {
                // Solo cerrados
                return await baseQuery
                    .Where(y => y.IsClosed)
                    .ToListAsync(cancellationToken);
            }

            // Abiertos + cerrados por Conceptos Automáticos
            var openQ = baseQuery.Where(y => !y.IsClosed);
            var closedAutoQ = baseQuery.Where(y => y.IsClosed && y.Source == YieldsSources.AutomaticConcept);

            return await openQ
                .Union(closedAutoQ)
                .OrderBy(y => y.YieldDetailId)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByPortfolioAndDateAsync(
            int portfolioId,
            DateTime closingDateUtc,
            bool isClosed = false,
            CancellationToken ct = default)
        {
            return await context.YieldDetails.AsNoTracking()
                .AnyAsync(y => y.PortfolioId == portfolioId &&
                               y.ClosingDate == closingDateUtc &&
                               y.IsClosed == isClosed, ct);
        }

        public async Task<int> InsertRangeImmediateAsync(IReadOnlyList<YieldDetail> items, CancellationToken cancellationToken = default)
        {
            if (items is null || items.Count == 0) return 0;

            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

            await db.YieldDetails.AddRangeAsync(items, cancellationToken);
            var inserted = await db.SaveChangesAsync(cancellationToken);

            return inserted;
        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetYieldDetailsAutConceptsAsync(IEnumerable<int> portfolioIdIds, DateTime closeDate, CancellationToken cancellationToken = default)
        {
            return await context.YieldDetails
                .AsNoTracking()
                .Where(y => portfolioIdIds.Contains(y.PortfolioId) && y.ClosingDate == closeDate && y.Source == YieldsSources.AutomaticConcept)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetYieldDetailsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds, DateTime closingDate, string source, CancellationToken cancellationToken = default)
        {
            return await context.YieldDetails
                .AsNoTracking()
                .Where(y => portfolioIds.Contains(y.PortfolioId)
                    && y.ClosingDate == closingDate
                    && y.IsClosed
                    && y.Source == source)
                .ToListAsync(cancellationToken);
        }
    }
}
