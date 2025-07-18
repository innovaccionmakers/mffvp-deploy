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
        /// <summary>
        /// Checks whether a portfolio valuation exists for the specified portfolio and closing date,
        /// and confirms that the valuation is already closed.
        /// </summary>
        /// <param name="portfolioId">The unique identifier of the portfolio.</param>
        /// <param name="closingDateUtc">The closing date in UTC to check for the valuation.</param>
        /// <param name="cancellationToken">Optional token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task that returns <c>true</c> if a closed portfolio valuation exists for the given
        /// portfolio and closing date; otherwise, <c>false</c>.
        /// </returns>
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
    }
}
