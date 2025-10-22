using Common.SharedKernel.Domain.ConfigurationParameters;

namespace Closing.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<IReadOnlyDictionary<Guid, ConfigurationParameter>> GetReadOnlyByUuidsAsync(
    IEnumerable<Guid> uuids,
    CancellationToken cancellationToken = default);
}
