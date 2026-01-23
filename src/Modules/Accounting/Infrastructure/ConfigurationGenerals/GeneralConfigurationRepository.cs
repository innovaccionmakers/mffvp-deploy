using Accounting.Domain.ConfigurationGenerals;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.ConfigurationGenerals;

internal sealed class GeneralConfigurationRepository(AccountingDbContext context) : IGeneralConfigurationRepository
{
    public Task<GeneralConfiguration?> GetGeneralConfigurationByPortfolioIdAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        return context.GeneralConfigurations.SingleOrDefaultAsync(gc => gc.PortfolioId == portfolioId, cancellationToken);
    }

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

    public void Insert(GeneralConfiguration generalConfiguration) => context.GeneralConfigurations.Add(generalConfiguration);

    public void Update(GeneralConfiguration generalConfiguration) => context.GeneralConfigurations.Update(generalConfiguration);

    public void Delete(GeneralConfiguration generalConfiguration) => context.GeneralConfigurations.Remove(generalConfiguration);

    public async Task<IReadOnlyCollection<GeneralConfiguration>> GetConfigurationGeneralsAsync(CancellationToken cancellationToken = default)
    {
        return await context.GeneralConfigurations.ToListAsync(cancellationToken);
    }
}

