using Associate.Domain.Activates;
using Associate.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Associate.Infrastructure;

internal sealed class ActivateRepository(AssociateDbContext context) : IActivateRepository
{
    public async Task<IReadOnlyCollection<Activate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Activates.ToListAsync(cancellationToken);
    }

    public void Insert(Activate activate, CancellationToken cancellationToken = default)
    {
        context.Activates.Add(activate);
    }    

    public void Update(Activate activate, CancellationToken cancellationToken = default)
    {
        context.Activates.Update(activate);
    }

    public async Task<Activate?> GetByIdTypeAndNumber(Guid IdentificationType, string identification, CancellationToken cancellationToken = default)
    {
        return await context.Activates.SingleOrDefaultAsync(a =>
            a.DocumentType == IdentificationType && a.Identification == identification);
    }

    public async Task<Activate?> GetByIdAsync(long activateId, CancellationToken cancellationToken = default)
    {
        return await context.Activates.SingleOrDefaultAsync(x => x.ActivateId == activateId, cancellationToken);
    }
}