using Common.SharedKernel.Domain.ConfigurationParameters;
namespace Customers.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<ConfigurationParameter?> GetByHomologationCodeAsync(string homologationCode,
        CancellationToken cancellationToken = default);

    Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ConfigurationParameter>> GetByCodesAndScopesAsync(
            IEnumerable<(string Code, string Scope)> parameters,
            CancellationToken cancellationToken = default);
}