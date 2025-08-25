﻿using System.Threading.Tasks;

namespace Closing.Domain.YieldDetails
{
    public interface IYieldDetailRepository
    {
        Task InsertAsync(YieldDetail yieldDetail, CancellationToken ct = default);

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

        Task<int> InsertRangeImmediateAsync(
            IReadOnlyList<YieldDetail> items, 
            CancellationToken cancellationToken = default);
    }
}
