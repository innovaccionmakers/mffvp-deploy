using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;

namespace Treasury.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ConfigurationParameter>> GetActiveConfigurationParametersByTypeAsync(
        ConfigurationParameterType type,
        CancellationToken cancellationToken = default);
}
