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

    Task<bool> UpsertAsync(
     int portfolioId,
     long trustId,
     DateTime processDate,
     decimal amount,
     long operationTypeId,
     long? clientOperationId,
     CancellationToken cancellationToken);
    }