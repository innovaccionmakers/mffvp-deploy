namespace Operations.Domain.TrustOperations;

public interface ITrustOperationRepository
{
    Task AddAsync(TrustOperation operation, CancellationToken cancellationToken);

    Task<TrustOperation?> GetByPortfolioAndDateAsync(
      int portfolioId,
      DateTime closingDate,
      CancellationToken cancellationToken);

    void Update(TrustOperation operation);
}