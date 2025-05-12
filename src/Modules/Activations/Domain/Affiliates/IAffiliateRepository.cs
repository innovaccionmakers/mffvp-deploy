namespace Activations.Domain.Affiliates;
public interface IAffiliateRepository
{
    Task<IReadOnlyCollection<Affiliate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Affiliate?> GetAsync(int id, CancellationToken cancellationToken = default);
    bool GetByIdTypeAndNumber(string IdentificationType, string identification);
    void Insert(Affiliate affiliate);
    void Update(Affiliate affiliate);
    void Delete(Affiliate affiliate);
}