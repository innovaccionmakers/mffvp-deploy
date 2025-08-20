using Closing.Domain.PortfolioValuations;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.PortfolioValuations
{
    internal sealed class PortfolioValuationRepository(ClosingDbContext context) : IPortfolioValuationRepository
    {
        public async Task<PortfolioValuation?> GetValuationAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
        {
            //Siempre se deben retornar valores cuyo indicador Cerrado = True,
            //si el campo para la fecha se encuentra en False quiere decir
            //que hace parte de un ejercicio de simulación el cual no debe considerarse como real.
            return await context.PortfolioValuations.Where(x => x.PortfolioId == portfolioId &&
                                                x.ClosingDate == closingDateUtc &&
                                                x.IsClosed == true)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ValuationExistsAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
        {
            return await context.PortfolioValuations
                .AnyAsync(x => x.PortfolioId == portfolioId &&
                               x.ClosingDate == closingDateUtc &&
                               x.IsClosed == true,
                          cancellationToken);
        }

        public async Task<bool> ExistsByPortfolioIdAsync(long portfolioId, CancellationToken cancellationToken = default)
        {
            return await context.PortfolioValuations
                .AnyAsync(x => x.PortfolioId == portfolioId,
                          cancellationToken);
        }

        public async Task AddAsync(PortfolioValuation valuation, CancellationToken cancellationToken = default)
        {
            await context.PortfolioValuations.AddAsync(valuation, cancellationToken);
        }

        public async Task DeleteClosedByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
        {
            await context.PortfolioValuations
                .Where(v => v.PortfolioId == portfolioId && v.ClosingDate == closingDateUtc && v.IsClosed)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<PortfolioValuation>> GetPortfolioValuationsByClosingDateAsync(DateTime closingDate, CancellationToken cancellationToken = default)
        {
            return await context.PortfolioValuations
                .AsNoTracking()
                .Where(v => v.ClosingDate == closingDate && v.IsClosed)
                .GroupBy(v => v.PortfolioId)
                .Select(g => g.First())
                .ToListAsync(cancellationToken);
        }
    }
}
