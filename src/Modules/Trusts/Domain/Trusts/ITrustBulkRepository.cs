using Trusts.Domain.Trusts.TrustYield;

namespace Trusts.Domain.Trusts;

public interface ITrustBulkRepository
{
    Task<ApplyYieldBulkResult> ApplyYieldToBalanceBulkAsync(
    IReadOnlyList<ApplyYieldRow> rows,
    CancellationToken cancellationToken = default);
}
