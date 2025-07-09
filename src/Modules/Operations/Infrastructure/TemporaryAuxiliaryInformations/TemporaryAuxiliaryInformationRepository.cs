using Microsoft.EntityFrameworkCore;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.TemporaryAuxiliaryInformations;

internal sealed class TemporaryAuxiliaryInformationRepository(OperationsDbContext context) : ITemporaryAuxiliaryInformationRepository
{
    public async Task<IReadOnlyCollection<TemporaryAuxiliaryInformation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TemporaryAuxiliaryInformations.ToListAsync(cancellationToken);
    }

    public async Task<TemporaryAuxiliaryInformation?> GetAsync(long temporaryAuxiliaryInformationId, CancellationToken cancellationToken = default)
    {
        return await context.TemporaryAuxiliaryInformations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TemporaryAuxiliaryInformationId == temporaryAuxiliaryInformationId, cancellationToken);
    }

    public void Insert(TemporaryAuxiliaryInformation temporaryAuxiliaryInformation)
    {
        context.TemporaryAuxiliaryInformations.Add(temporaryAuxiliaryInformation);
    }

    public void Update(TemporaryAuxiliaryInformation temporaryAuxiliaryInformation)
    {
        context.TemporaryAuxiliaryInformations.Update(temporaryAuxiliaryInformation);
    }

    public void Delete(TemporaryAuxiliaryInformation temporaryAuxiliaryInformation)
    {
        context.TemporaryAuxiliaryInformations.Remove(temporaryAuxiliaryInformation);
    }
    public void DeleteRange(IEnumerable<TemporaryAuxiliaryInformation> infos)
    {
        context.TemporaryAuxiliaryInformations.RemoveRange(infos);
    }

    public async Task<IReadOnlyCollection<TemporaryAuxiliaryInformation>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        return await context.TemporaryAuxiliaryInformations
            .Where(x => ids.Contains(x.TemporaryAuxiliaryInformationId))
            .ToListAsync(cancellationToken);
    }
}
