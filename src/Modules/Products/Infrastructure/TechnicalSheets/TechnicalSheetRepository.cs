using Products.Domain.TechnicalSheets;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.TechnicalSheets;

internal class TechnicalSheetRepository(ProductsDbContext context) : ITechnicalSheetRepository
{
    public async Task AddAsync(TechnicalSheet technicalSheet, CancellationToken cancellationToken = default)
    {
        await context.TechnicalSheets.AddAsync(technicalSheet, cancellationToken);
    }
}
