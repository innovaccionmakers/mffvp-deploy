using Microsoft.EntityFrameworkCore;
using Treasury.Domain.Issuers;
using Treasury.Infrastructure.Database;

namespace Treasury.Infrastructure.Issuers;

public class IssuerRepository(TreasuryDbContext context) : IIssuerRepository
{
    public async Task<Issuer?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.Issuers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Issuer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Issuers.ToListAsync(cancellationToken);
    }

    public void Add(Issuer issuer)
    {
        context.Issuers.Add(issuer);
    }
}