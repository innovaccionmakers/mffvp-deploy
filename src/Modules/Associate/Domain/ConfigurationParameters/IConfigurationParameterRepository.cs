using Common.SharedKernel.Domain.ConfigurationParameters;
namespace Associate.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationParameter?> GetAsync(Guid Uuid, CancellationToken cancellationToken = default);
    void Insert(ConfigurationParameter configurationparameter);
    void Update(ConfigurationParameter configurationparameter);
    Task<ConfigurationParameter?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    Task<ConfigurationParameter> GetByCodeAndScopeAsync(string homologationCode, string scope, CancellationToken cancellationToken = default);
}