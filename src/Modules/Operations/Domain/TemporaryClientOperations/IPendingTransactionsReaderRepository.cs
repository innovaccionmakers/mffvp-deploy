

namespace Operations.Domain.TemporaryClientOperations;

public interface IPendingTransactionsReaderRepository
{
    Task<IReadOnlyList<PendingContributionRow>> TakePendingBatchWithAuxAsync(
     int portfolioId, int batchSize, CancellationToken cancellationToken);


}
