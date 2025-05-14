namespace Trusts.Domain.TrustHistories;

public interface ITrustHistoryRepository
{
    Task<IReadOnlyCollection<TrustHistory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TrustHistory?> GetAsync(long trusthistoryId, CancellationToken cancellationToken = default);
    void Insert(TrustHistory trusthistory);
    void Update(TrustHistory trusthistory);
    void Delete(TrustHistory trusthistory);
}