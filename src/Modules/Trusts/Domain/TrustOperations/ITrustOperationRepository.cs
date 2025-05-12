namespace Trusts.Domain.TrustOperations;

public interface ITrustOperationRepository
{
    Task<IReadOnlyCollection<TrustOperation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TrustOperation?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(TrustOperation trustoperation);
    void Update(TrustOperation trustoperation);
    void Delete(TrustOperation trustoperation);
}