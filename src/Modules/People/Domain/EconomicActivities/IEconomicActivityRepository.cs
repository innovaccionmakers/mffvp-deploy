namespace People.Domain.EconomicActivities;
public interface IEconomicActivityRepository
{
    Task<IReadOnlyCollection<EconomicActivity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EconomicActivity?> GetAsync(string economicactivityId, CancellationToken cancellationToken = default);
    void Insert(EconomicActivity economicactivity);
    void Update(EconomicActivity economicactivity);
    void Delete(EconomicActivity economicactivity);
}