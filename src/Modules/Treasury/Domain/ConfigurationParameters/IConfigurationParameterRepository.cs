using Common.SharedKernel.Domain.ConfigurationParameters;

namespace Treasury.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);
}
