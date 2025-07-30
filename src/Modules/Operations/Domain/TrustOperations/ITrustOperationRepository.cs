namespace Operations.Domain.TrustOperations;

public interface ITrustOperationRepository
{
    Task AddAsync(TrustOperation operation, CancellationToken cancellationToken);
}