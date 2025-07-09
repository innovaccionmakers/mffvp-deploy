namespace Operations.Domain.TemporaryClientOperations;

public interface ITemporaryClientOperationRepository
{
    Task<IReadOnlyCollection<TemporaryClientOperation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TemporaryClientOperation?> GetAsync(long temporaryClientOperationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TemporaryClientOperation>> GetByPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default);
    void Insert(TemporaryClientOperation temporaryClientOperation);
    void Update(TemporaryClientOperation temporaryClientOperation);
    void Delete(TemporaryClientOperation temporaryClientOperation);
}
