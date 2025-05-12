using Microsoft.EntityFrameworkCore;
using Activations.Domain.Affiliates;
using Activations.Infrastructure.Database;

namespace Activations.Infrastructure;

internal sealed class AffiliateRepository(ActivationsDbContext context) : IAffiliateRepository
{
    public async Task<IReadOnlyCollection<Affiliate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Affiliates.ToListAsync(cancellationToken);
    }

    public async Task<Affiliate?> GetAsync(int affiliateId, CancellationToken cancellationToken = default)
    {
        return await context.Affiliates
            .SingleOrDefaultAsync(x => x.AffiliateId == affiliateId, cancellationToken);
    }

    public void Insert(Affiliate affiliate)
    {
        context.Affiliates.Add(affiliate);
    }

    public void Update(Affiliate affiliate)
    {
        context.Affiliates.Update(affiliate);
    }

    public void Delete(Affiliate affiliate)
    {
        context.Affiliates.Remove(affiliate);
    }

    public bool GetByIdTypeAndNumber(string IdentificationType, string identification)
        => context.Affiliates.Any(a => a.IdentificationType == IdentificationType && a.Identification == identification);
}