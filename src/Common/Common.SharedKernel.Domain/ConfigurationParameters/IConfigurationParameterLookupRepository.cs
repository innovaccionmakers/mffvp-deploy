namespace Common.SharedKernel.Domain.ConfigurationParameters;

public interface IConfigurationParameterLookupRepository
{
    Task<ConfigurationParameter?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
}

public interface IConfigurationParameterLookupRepository<TModule> : IConfigurationParameterLookupRepository
{
}
