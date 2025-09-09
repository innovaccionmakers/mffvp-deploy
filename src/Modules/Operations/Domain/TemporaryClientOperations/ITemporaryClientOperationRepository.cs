namespace Operations.Domain.TemporaryClientOperations;

public interface ITemporaryClientOperationRepository
{
    Task<IReadOnlyCollection<TemporaryClientOperation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TemporaryClientOperation?> GetAsync(long temporaryClientOperationId, CancellationToken cancellationToken = default);
    Task<TemporaryClientOperation?> GetForUpdateAsync(long temporaryClientOperationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TemporaryClientOperation>> GetByPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TemporaryClientOperation>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
    Task<int> DeleteByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
    void Insert(TemporaryClientOperation temporaryClientOperation);
    void Update(TemporaryClientOperation temporaryClientOperation);
    void Delete(TemporaryClientOperation temporaryClientOperation);
    void DeleteRange(IEnumerable<TemporaryClientOperation> operations);

    Task<long?> GetNextPendingIdAsync(
   int portfolioId,
   CancellationToken cancellationToken = default);

    Task<int> MarkProcessedIfPendingAsync(
    long temporaryClientOperationId,
    CancellationToken cancellationToken = default);
}
