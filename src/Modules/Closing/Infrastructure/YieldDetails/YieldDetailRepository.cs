using Closing.Domain.YieldDetails;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;


namespace Closing.Infrastructure.YieldDetails
{
    internal sealed class YieldDetailRepository(ClosingDbContext context) : IYieldDetailRepository
    {
        public async Task<IReadOnlyCollection<YieldDetail>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.YieldDetails.ToListAsync(cancellationToken);
        }
        public async Task<YieldDetail?> GetAsync(long yieldDetailId, CancellationToken cancellationToken = default)
        {
            return await context.YieldDetails
                .SingleOrDefaultAsync(x => x.YieldDetailId == yieldDetailId, cancellationToken);
        }
        public void Insert(YieldDetail yieldDetail)
        {
            context.YieldDetails.Add(yieldDetail);
        }
        public async Task InsertAsync(YieldDetail yieldDetail, CancellationToken ct = default)
        {
            await context.YieldDetails.AddAsync(yieldDetail, ct);
        }

        public void Update(YieldDetail yieldDetail)
        {
            context.YieldDetails.Update(yieldDetail);
        }

        public void Delete(YieldDetail yieldDetail)
        {
            context.YieldDetails.Remove(yieldDetail);

        }

        public async Task DeleteByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
        {
            var deletedCount = await context.YieldDetails
                .Where(yield => yield.PortfolioId == portfolioId
                             && yield.ClosingDate == closingDateUtc
                             && !yield.IsClosed) //Solo se pueden borrar los que no son Cerrados
                .ExecuteDeleteAsync(cancellationToken);

        }
        
        public async Task DeleteClosedByPortfolioAndDateAsync(
            int portfolioId,
            DateTime closingDateUtc,
            CancellationToken cancellationToken = default)
        {
            await context.YieldDetails
                .Where(yield => yield.PortfolioId == portfolioId
                                && yield.ClosingDate == closingDateUtc
                                && yield.IsClosed)
                .ExecuteDeleteAsync(cancellationToken);

        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        bool isClosed = false,
        CancellationToken ct = default)
        {
            return await context.YieldDetails
                .Where(y => y.PortfolioId == portfolioId && y.ClosingDate == closingDateUtc && y.IsClosed == isClosed)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByPortfolioAndDateAsync(
            int portfolioId,
            DateTime closingDateUtc,
            bool isClosed = false,
            CancellationToken ct = default)
        {
            return await context.YieldDetails
                .AnyAsync(y => y.PortfolioId == portfolioId &&
                               y.ClosingDate == closingDateUtc &&
                               y.IsClosed == isClosed, ct);
        }


    }
}
