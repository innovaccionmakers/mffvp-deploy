namespace Associate.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationParameter?> GetAsync(int id, CancellationToken cancellationToken = default);
    void Insert(ConfigurationParameter configurationparameter);
    void Update(ConfigurationParameter configurationparameter);
    Task<ConfigurationParameter?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    Task<bool> GetByCodeAndScopeAsync(string homologationCode, string scope, CancellationToken cancellationToken = default);
}