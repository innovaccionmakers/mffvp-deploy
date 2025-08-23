using System.Threading.Tasks;

namespace Closing.Domain.YieldDetails
{
    public interface IYieldDetailRepository
    {
        Task InsertAsync(YieldDetail yieldDetail, CancellationToken ct = default);
        void Update(YieldDetail yieldDetail);
        void Delete(YieldDetail yieldDetail);
        Task<IReadOnlyCollection<YieldDetail>> GetReadOnlyByPortfolioAndDateAsync(
          int portfolioId,
          DateTime closingDateUtc,
          bool isClosed = false,
          CancellationToken ct = default);
        Task<bool> ExistsByPortfolioAndDateAsync(
           int portfolioId,
           DateTime closingDateUtc,
           bool isClosed = false,
           CancellationToken ct = default);

        Task DeleteByPortfolioAndDateAsync(
           int portfolioId,
           DateTime closingDateUtc,
           CancellationToken cancellationToken = default);
        
        Task DeleteClosedByPortfolioAndDateAsync(
            int portfolioId,
            DateTime closingDateUtc,
            CancellationToken cancellationToken = default);

    }
}
