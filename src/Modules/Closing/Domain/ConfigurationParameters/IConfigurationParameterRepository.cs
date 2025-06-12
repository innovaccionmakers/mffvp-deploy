using Common.SharedKernel.Domain.ConfigurationParameters;

namespace Closing.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository
{
    Task<ConfigurationParameter?> GetByUuidAsync(
        Guid uuid,
        CancellationToken cancellationToken = default
    );
}