using System.Threading;
using System.Threading.Tasks;

namespace Closing.Domain.YieldsToDistribute;

public interface IYieldToDistributeRepository
{
    Task InsertRangeAsync(IEnumerable<YieldToDistribute> yieldsToDistribute, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
