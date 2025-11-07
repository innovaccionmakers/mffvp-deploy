using System.Threading;
using System.Threading.Tasks;

namespace Closing.Domain.YieldsToDistribute;

public interface IYieldToDistributeRepository
{
    Task InsertRangeAsync(IEnumerable<YieldToDistribute> yieldsToDistribute, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<YieldToDistribute>> GetReadOnlyByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default);
    
    Task<decimal> GetTotalYieldAmountRoundedAsync(
        int portfolioId,
        DateTime closingDateUtc,
        string? conceptJson = null,
        CancellationToken cancellationToken = default);
}
