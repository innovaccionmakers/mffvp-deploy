namespace Operations.Domain.TrustOperations;

public interface ITrustOperationBulkRepository
{
    Task<UpsertBulkResult> UpsertBulkAsync(
        int portfolioId,
        IReadOnlyList<TrustYieldOpRowForBulk> trustYieldOperations,
        CancellationToken cancellationToken);

}
