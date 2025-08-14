using Microsoft.EntityFrameworkCore;
using System.Linq;
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

    public async Task<IReadOnlyCollection<Issuer>> GetBanksAsync(CancellationToken cancellationToken = default)
    {
        return await context.Issuers
            .Where(x => x.IsBank)
            .ToListAsync(cancellationToken);
    }
    
    public Task<Issuer?> GetByHomologatedCodeAsync(string homologatedCode, CancellationToken cancellationToken = default)
    {
        return context.Issuers.SingleOrDefaultAsync(
            x => x.HomologatedCode == homologatedCode,
            cancellationToken);
    }

    public void Add(Issuer issuer)
    {
        context.Issuers.Add(issuer);
    }
}