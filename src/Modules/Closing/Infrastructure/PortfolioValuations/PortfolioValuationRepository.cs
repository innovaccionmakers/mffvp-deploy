using Closing.Domain.PortfolioValuations;
using Closing.Infrastructure.Database;
using Common.SharedKernel.Domain.Utils;
using Microsoft.EntityFrameworkCore;


namespace Closing.Infrastructure.PortfolioValuations
{
    internal sealed class PortfolioValuationRepository(ClosingDbContext context) : IPortfolioValuationRepository
    {
        public async Task<PortfolioValuation?> GetValuationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default)
        {
            //Siempre se deben retornar valores cuyo indicador Cerrado = True,
            //si el campo para la fecha se encuentra en False quiere decir
            //que hace parte de un ejercicio de simulación el cual no debe considerarse como real.
            var closingDateUtc = DateTimeConverter.ToUtcDateTime(closingDate);
            return await context.PortfolioValuations.Where(x => x.PortfolioId == portfolioId &&
                                                x.ClosingDate == closingDateUtc &&
                                                x.IsClosed == true) 
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
