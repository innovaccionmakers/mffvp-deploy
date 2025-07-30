using Microsoft.EntityFrameworkCore;
using Products.Domain.AccumulatedCommissions;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.AccumulatedCommissions;
internal sealed class AccumulatedCommissionRepository(
    ProductsDbContext context
) : IAccumulatedCommissionRepository
{
    public async Task<AccumulatedCommission?> GetByPortfolioAndCommissionAsync(
        int portfolioId,
        int commissionId,
        CancellationToken cancellationToken = default)
    {
        return await context
            .AccumulatedCommissions
            .SingleOrDefaultAsync(x =>
                x.PortfolioId == portfolioId &&
                x.CommissionId == commissionId,
                cancellationToken);
    }

    public async Task AddAsync(
        AccumulatedCommission commission,
        CancellationToken cancellationToken)
    {
        await context.AccumulatedCommissions.AddAsync(commission, cancellationToken);
    }
}