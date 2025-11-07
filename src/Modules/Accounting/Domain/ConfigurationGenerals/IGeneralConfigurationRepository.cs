namespace Accounting.Domain.ConfigurationGenerals;

public interface IGeneralConfigurationRepository
{
    Task<IEnumerable<GeneralConfiguration>> GetGeneralConfigurationsByPortfolioIdsAsync(
        IEnumerable<int> portfolioIds,
        CancellationToken cancellationToken = default);
}

