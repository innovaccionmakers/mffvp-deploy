namespace Accounting.Domain.ConfigurationGenerals;

public interface IGeneralConfigurationRepository
{
    Task<IEnumerable<GeneralConfiguration>> GetGeneralConfigurationsByPortfolioIdsAsync(
        IEnumerable<int> portfolioIds,
        CancellationToken cancellationToken = default);

    Task<GeneralConfiguration?> GetGeneralConfigurationByPortfolioIdAsync(int portfolioId, CancellationToken cancellationToken = default);
    void Insert(GeneralConfiguration generalConfiguration);
    void Update(GeneralConfiguration generalConfiguration);
    void Delete(GeneralConfiguration generalConfiguration);
}

