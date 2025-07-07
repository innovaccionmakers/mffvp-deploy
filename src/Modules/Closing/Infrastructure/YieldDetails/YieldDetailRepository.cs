using Closing.Domain.YieldDetails;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;


namespace Closing.Infrastructure.YieldDetails
{
    internal sealed class YieldDetailRepository(ClosingDbContext context) : IYieldDetailRepository
    {
        public async Task<IReadOnlyCollection<YieldDetail>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.YieldDetails.ToListAsync(cancellationToken);
        }
        public async Task<YieldDetail?> GetAsync(long yieldDetailId, CancellationToken cancellationToken = default)
        {
            return await context.YieldDetails
                .SingleOrDefaultAsync(x => x.YieldDetailId == yieldDetailId, cancellationToken);
        }
        public void Insert(YieldDetail yieldDetail)
        {
            context.YieldDetails.Add(yieldDetail);
        }
        public void InsertAsync(YieldDetail yieldDetail)
        {
            context.YieldDetails.AddAsync(yieldDetail);
        }
        public void Update(YieldDetail yieldDetail)
        {
            context.YieldDetails.Update(yieldDetail);
        }

        public void Delete(YieldDetail yieldDetail)
        {
            context.YieldDetails.Remove(yieldDetail);

        }
    }
}
