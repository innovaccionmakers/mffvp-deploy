

namespace Closing.Domain.TrustYields;

public interface ITrustYieldBulkRepository
{
    Task BulkUpdateAsync(
     IReadOnlyCollection<TrustYieldUpdateRow> trustYieldRow,
     CancellationToken cancellationToken);
}
