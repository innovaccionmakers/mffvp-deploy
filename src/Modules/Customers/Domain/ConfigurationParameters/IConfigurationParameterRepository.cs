namespace Customers.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository
{
    Task<ConfigurationParameter?> GetByUuidAsync(
        Guid uuid,
        CancellationToken cancellationToken = default
    );

    Task<ConfigurationParameter?> GetByHomologationCodeAsync(string homologationCode,
        CancellationToken cancellationToken = default);

    Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default);
}