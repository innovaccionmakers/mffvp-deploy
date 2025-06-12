using Common.SharedKernel.Domain.ConfigurationParameters;
namespace Products.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationParameter?> GetAsync(int configurationParameterId, CancellationToken cancellationToken = default);
    void Insert(ConfigurationParameter parameter);
    void Update(ConfigurationParameter parameter);
    void Delete(ConfigurationParameter parameter);

    Task<IReadOnlyCollection<ConfigurationParameter>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken ct = default);

    Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default);
}