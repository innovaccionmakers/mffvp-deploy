namespace Customers.Domain.EconomicActivities;
public interface IEconomicActivityRepository
{
    Task<IReadOnlyCollection<EconomicActivity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EconomicActivity?> GetAsync(string homologatedCode, CancellationToken cancellationToken = default);
    void Insert(EconomicActivity economicactivity);
    void Update(EconomicActivity economicactivity);
    void Delete(EconomicActivity economicactivity);
}