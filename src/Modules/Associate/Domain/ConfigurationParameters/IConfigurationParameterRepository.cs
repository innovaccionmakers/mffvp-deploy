using Common.SharedKernel.Domain.ConfigurationParameters;
namespace Associate.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationParameter?> GetAsync(Guid Uuid, CancellationToken cancellationToken = default);
    void Insert(ConfigurationParameter configurationparameter);
    void Update(ConfigurationParameter configurationparameter);
    Task<ConfigurationParameter> GetByCodeAndScopeAsync(string homologationCode, string scope, CancellationToken cancellationToken = default);
}