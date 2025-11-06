using Accounting.Domain.ConfigurationGenerals;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.ConfigurationGenerals;

internal sealed class GeneralConfigurationRepository(AccountingDbContext context) : IGeneralConfigurationRepository
{
    public async Task<IEnumerable<GeneralConfiguration>> GetGeneralConfigurationsByPortfolioIdsAsync(
        IEnumerable<int> portfolioIds,
        CancellationToken cancellationToken = default)
    {
        if (portfolioIds == null || !portfolioIds.Any())
            return Enumerable.Empty<GeneralConfiguration>();

        var portfolioIdsSet = new HashSet<int>(portfolioIds);

        return await context.GeneralConfigurations
            .Where(gc => portfolioIdsSet.Contains(gc.PortfolioId))
            .ToListAsync(cancellationToken);
    }
}

