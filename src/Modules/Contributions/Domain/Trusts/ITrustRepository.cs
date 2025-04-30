namespace Contributions.Domain.Trusts;
public interface ITrustRepository
{
    Task<IReadOnlyCollection<Trust>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Trust?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(Trust trust);
    void Update(Trust trust);
    void Delete(Trust trust);
}