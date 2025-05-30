namespace Products.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository
{
    Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationParameter?> GetAsync(int configurationParameterId, CancellationToken cancellationToken = default);
    void Insert(ConfigurationParameter parameter);
    void Update(ConfigurationParameter parameter);
    void Delete(ConfigurationParameter parameter);

    Task<ConfigurationParameter?> GetByUuidAsync(
        Guid uuid,
        CancellationToken cancellationToken = default
    );
    
    Task<IReadOnlyCollection<ConfigurationParameter>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken ct = default);
}