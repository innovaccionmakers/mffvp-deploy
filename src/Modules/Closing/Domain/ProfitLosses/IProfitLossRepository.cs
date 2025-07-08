namespace Closing.Domain.ProfitLosses;

public interface IProfitLossRepository
{
    Task<IReadOnlyCollection<ProfitLoss>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProfitLoss?> GetAsync(long profitLossId, CancellationToken cancellationToken = default);
    Task DeleteByPortfolioAndDateAsync(int portfolioId, DateTime effectiveDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfitLossSummary>> GetSummaryAsync(int portfolioId, DateTime effectiveDate, CancellationToken cancellationToken = default);
    void InsertRange(IEnumerable<ProfitLoss> profitLosses);
    Task<IReadOnlyList<ProfitLossConceptSummary>> GetConceptSummaryAsync(int portfolioId,
                                                                      DateTime effectiveDate,
                                                                      CancellationToken cancellationToken = default);
}