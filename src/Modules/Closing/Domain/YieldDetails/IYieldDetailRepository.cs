namespace Closing.Domain.YieldDetails
{
    public interface IYieldDetailRepository
    {
        Task<IReadOnlyCollection<YieldDetail>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<YieldDetail?> GetAsync(long yieldDetailId, CancellationToken cancellationToken = default);
        void Insert(YieldDetail yieldDetail);
        Task InsertAsync(YieldDetail yieldDetail, CancellationToken ct = default);
        void Update(YieldDetail yieldDetail);
        void Delete(YieldDetail yieldDetail);
        Task<IReadOnlyCollection<YieldDetail>> GetByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken ct = default);

    }
}
