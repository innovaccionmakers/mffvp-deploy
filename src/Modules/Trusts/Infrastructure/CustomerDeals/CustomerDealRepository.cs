using Microsoft.EntityFrameworkCore;
using Trusts.Domain.CustomerDeals;
using Trusts.Infrastructure.Database;

namespace Trusts.Infrastructure;

internal sealed class CustomerDealRepository(TrustsDbContext context) : ICustomerDealRepository
{
    public async Task<IReadOnlyCollection<CustomerDeal>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.CustomerDeals.ToListAsync(cancellationToken);
    }

    public async Task<CustomerDeal?> GetAsync(Guid customerdealId, CancellationToken cancellationToken = default)
    {
        return await context.CustomerDeals
            .SingleOrDefaultAsync(x => x.CustomerDealId == customerdealId, cancellationToken);
    }

    public void Insert(CustomerDeal customerdeal)
    {
        context.CustomerDeals.Add(customerdeal);
    }

    public void Update(CustomerDeal customerdeal)
    {
        context.CustomerDeals.Update(customerdeal);
    }

    public void Delete(CustomerDeal customerdeal)
    {
        context.CustomerDeals.Remove(customerdeal);
    }
}