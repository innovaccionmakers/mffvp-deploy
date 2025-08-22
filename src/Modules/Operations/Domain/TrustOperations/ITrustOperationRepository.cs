namespace Operations.Domain.TrustOperations;

public interface ITrustOperationRepository
{
    Task AddAsync(TrustOperation operation, CancellationToken cancellationToken);

    Task<TrustOperation?> GetForUpdateByPortfolioTrustAndDateAsync(
       int portfolioId,
       long trustId,
       DateTime closingDate,
       CancellationToken cancellationToken);

    void Update(TrustOperation operation);

    Task<TrustOperation?> GetByPortfolioAndTrustAsync(
      int portfolioId,
      long trustId,
      DateTime closingDate,
      CancellationToken cancellationToken);
}