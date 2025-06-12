using Common.SharedKernel.Domain.ConfigurationParameters;
namespace Trusts.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationParameter?> GetAsync(int configurationParameterId, CancellationToken cancellationToken = default);
    void Insert(ConfigurationParameter parameter);
    void Update(ConfigurationParameter parameter);
    void Delete(ConfigurationParameter parameter);
}