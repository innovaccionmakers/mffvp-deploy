using Microsoft.EntityFrameworkCore;
using Operations.Domain.AuxiliaryInformations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.AuxiliaryInformations;

internal sealed class AuxiliaryInformationRepository(OperationsDbContext context) : IAuxiliaryInformationRepository
{
    public async Task<IReadOnlyCollection<AuxiliaryInformation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.AuxiliaryInformations.ToListAsync(cancellationToken);
    }

    public async Task<AuxiliaryInformation?> GetAsync(long auxiliaryinformationId,
        CancellationToken cancellationToken = default)
    {
        return await context.AuxiliaryInformations
            .SingleOrDefaultAsync(x => x.AuxiliaryInformationId == auxiliaryinformationId, cancellationToken);
    }

    public void Insert(AuxiliaryInformation auxiliaryinformation)
    {
        context.AuxiliaryInformations.Add(auxiliaryinformation);
    }

    public void Update(AuxiliaryInformation auxiliaryinformation)
    {
        context.AuxiliaryInformations.Update(auxiliaryinformation);
    }

    public void Delete(AuxiliaryInformation auxiliaryinformation)
    {
        context.AuxiliaryInformations.Remove(auxiliaryinformation);
    }
}