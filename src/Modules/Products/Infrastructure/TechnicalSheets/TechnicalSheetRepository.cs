using Products.Domain.TechnicalSheets;
using Products.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Products.Infrastructure.TechnicalSheets;

internal class TechnicalSheetRepository(ProductsDbContext context) : ITechnicalSheetRepository
{
    public async Task AddAsync(TechnicalSheet technicalSheet, CancellationToken cancellationToken = default)
    {
        await context.TechnicalSheets.AddAsync(technicalSheet, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TechnicalSheet> technicalSheets, CancellationToken cancellationToken = default)
    {
        await context.TechnicalSheets.AddRangeAsync(technicalSheets, cancellationToken);
    }

    public async Task<IEnumerable<TechnicalSheet>> GetByDateRangeAndPortfolioAsync(
        DateOnly startDate,
        DateOnly endDate,
        int portfolioId,
        CancellationToken cancellationToken = default)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue).ToUniversalTime();

        return await context.TechnicalSheets
            .Where(ts => ts.PortfolioId == portfolioId &&
                         ts.Date >= startDateTime &&
                         ts.Date <= endDateTime)
            .OrderBy(ts => ts.Date)
            .ToListAsync(cancellationToken);
    }


    public async Task<bool> ExistsByDateAsync(DateTime closingDate, CancellationToken cancellationToken = default)
    {
        var closingDateUtc = closingDate.ToUniversalTime();
        return await context.TechnicalSheets
            .AnyAsync(ts => ts.Date == closingDateUtc.Date, cancellationToken);
    }

    public async Task DeleteByDateAsync(DateTime closingDate, CancellationToken cancellationToken = default)
    {
        var technicalSheetsToDelete = await context.TechnicalSheets
            .Where(ts => ts.Date == closingDate.Date)
            .ToListAsync(cancellationToken);

        if (technicalSheetsToDelete.Count != 0)
        {
            context.TechnicalSheets.RemoveRange(technicalSheetsToDelete);
        }
    }
}
