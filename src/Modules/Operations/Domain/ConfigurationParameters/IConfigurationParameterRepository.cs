namespace Operations.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository
{
    Task<ConfigurationParameter?> GetByUuidAsync(
        Guid uuid,
        CancellationToken cancellationToken = default
    );
    
    Task<IReadOnlyDictionary<(string Code, string Scope), ConfigurationParameter>>
        GetByCodesAndTypesAsync(
            IEnumerable<(string Code, string Scope)> pairs,
            CancellationToken cancellationToken = default);


    Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyDictionary<Guid, ConfigurationParameter>> 
        GetByUuidsAsync(IEnumerable<Guid> uuids, CancellationToken cancellationToken = default);
}