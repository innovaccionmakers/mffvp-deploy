namespace Closing.Domain.YieldDetails
{
    public interface IYieldDetailRepository
    {
        Task<IReadOnlyCollection<YieldDetail>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<YieldDetail?> GetAsync(long yieldDetailId, CancellationToken cancellationToken = default);
        void Insert(YieldDetail yieldDetail);
        void InsertAsync(YieldDetail yieldDetail);
        void Update(YieldDetail yieldDetail);
        void Delete(YieldDetail yieldDetail);
    }
}
