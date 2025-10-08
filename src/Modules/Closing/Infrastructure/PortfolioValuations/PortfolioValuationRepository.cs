using Closing.Domain.PortfolioValuations;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.PortfolioValuations
{
    internal sealed class PortfolioValuationRepository(ClosingDbContext context) : IPortfolioValuationRepository
    {
        public async Task<PortfolioValuation?> GetReadOnlyByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
        {
            return await context.PortfolioValuations
                .AsNoTracking()
                .TagWith("PortfolioValuationRepository_GetReadOnlyByPortfolioAndDateAsync")
                .Where(x => x.PortfolioId == portfolioId &&
                                                x.ClosingDate == closingDateUtc &&
                                                x.IsClosed == true)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ExistsByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
        {
            return await context.PortfolioValuations
                .AsNoTracking()
                 .TagWith("PortfolioValuationRepository_ExistsByPortfolioAndDateAsync")
                .AnyAsync(x => x.PortfolioId == portfolioId &&
                               x.ClosingDate == closingDateUtc &&
                               x.IsClosed == true,
                          cancellationToken);
        }

        public async Task<bool> ExistsByPortfolioIdAsync(long portfolioId, CancellationToken cancellationToken = default)
        {
            return await context.PortfolioValuations.AsNoTracking()
                .TagWith("PortfolioValuationRepository_ExistsByPortfolioIdAsync")
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
                .TagWith("PortfolioValuationRepository_DeleteClosedByPortfolioAndDateAsync")
                .Where(v => v.PortfolioId == portfolioId && v.ClosingDate == closingDateUtc && v.IsClosed)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<PortfolioValuation>> GetPortfolioValuationsByClosingDateAsync(DateTime closingDate, CancellationToken cancellationToken = default)
        {
            return await context.PortfolioValuations
                .AsNoTracking()
                .TagWith("PortfolioValuationRepository_GetPortfolioValuationsByClosingDateAsync")
                .Where(v => v.ClosingDate == closingDate && v.IsClosed)
                .GroupBy(v => v.PortfolioId)
                .Select(g => g.First())
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Aplica la diferencia de rendimientos (positiva o negativa) sobre la valoración del portafolio
        /// del día de cierre indicado, cuando el registro está cerrado. Ajusta:
        ///   - valor = valor + difference
        ///   - unidades = (valor + difference) / valor_unidad    (si valor_unidad != 0)
        ///   - fecha_proceso = NOW()
        /// Devuelve la cantidad de filas actualizadas (0 o 1).
        /// </summary>
        public async Task<int> ApplyAllocationCheckDiffAsync(
            int portfolioId,
            DateTime closingDateUtc,
            decimal difference,
            CancellationToken cancellationToken)
        {
            if (difference == 0m)
                return 0;

            var rowsAffected = await context.PortfolioValuations
                .Where(x => x.PortfolioId == portfolioId
                            && x.ClosingDate == closingDateUtc
                            && x.IsClosed)
                .TagWith("[PortfolioValuationRepository_ApplyAllocationCheckDiff_UpdateAmountAndUnits]")
                .ExecuteUpdateAsync(setters => setters
                    // valor = valor + difference (suma o resta según el signo)
                    .SetProperty(p => p.Amount, p => p.Amount + difference)
                    // unidades = (valor + difference) / valor_unidad, evitando división por cero
                    .SetProperty(p => p.Units, p => p.UnitValue == 0m
                                                        ? p.Units
                                                        : (p.Amount + difference) / p.UnitValue)
                    // fecha_proceso = NOW()
                    .SetProperty(p => p.ProcessDate, _ => DateTime.UtcNow),
                    cancellationToken);

            return rowsAffected;
        }
    }
}
