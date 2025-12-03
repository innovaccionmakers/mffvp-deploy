using Closing.Domain.YieldsToDistribute;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.YieldsToDistribute;

internal sealed class YieldToDistributeRepository(ClosingDbContext context) : IYieldToDistributeRepository
{
    public async Task InsertRangeAsync(IEnumerable<YieldToDistribute> yieldsToDistribute, CancellationToken cancellationToken = default)
    {
        await context.Set<YieldToDistribute>()
            .AddRangeAsync(yieldsToDistribute, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<YieldToDistribute>> GetReadOnlyByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<YieldToDistribute>()
            .AsNoTracking()
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate == closingDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalYieldAmountRoundedAsync(
        int portfolioId,
        DateTime closingDateUtc,
        string? conceptJson = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Set<YieldToDistribute>()
            .AsNoTracking()
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate == closingDateUtc);

        // Filtrar por concepto JSONB completo si se proporciona
        if (!string.IsNullOrEmpty(conceptJson))
        {
            query = query.Where(y => EF.Functions.JsonContained(y.Concept, conceptJson));
        }

        return await query
            .Select(y => Math.Round(y.YieldAmount, 2))
            .TagWith("YieldToDistributeRepository_GetTotalYieldAmountRoundedAsync")
            .SumAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<YieldToDistribute>> GetDistributedYieldsByConceptAsync(IEnumerable<int> portfolioIds,
                                                                                           DateTime closingDateUtc,
                                                                                           string? concept,
                                                                                           CancellationToken cancellationToken = default)
    {
        var query = context.YieldsToDistribute
            .AsNoTracking()
            .Where(y => portfolioIds.Contains(y.PortfolioId) && y.ClosingDate == closingDateUtc);

        if (!string.IsNullOrEmpty(concept))
        {
            query = query.Where(y => EF.Functions.JsonContained(y.Concept, concept));
        }

        return await query.ToListAsync(cancellationToken);
    }
}
