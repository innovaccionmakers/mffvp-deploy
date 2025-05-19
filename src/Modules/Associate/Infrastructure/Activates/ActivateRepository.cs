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

    public void Insert(Activate activate)
    {
        context.Activates.Add(activate);
    }

    public bool GetByIdTypeAndNumber(string IdentificationType, string identification)
    {
        return context.Activates.Any(a =>
            a.IdentificationType == IdentificationType && a.Identification == identification);
    }
}