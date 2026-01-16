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

    public async Task<Dictionary<long, int>> GetCollectionBankIdsByClientOperationIdsAsync(
        IEnumerable<long> clientOperationIds,
        CancellationToken cancellationToken = default)
    {
        if (clientOperationIds == null || !clientOperationIds.Any())
            return new Dictionary<long, int>();

        var clientOperationIdsList = clientOperationIds.Distinct().ToList();

        return await context.AuxiliaryInformations
            .Where(ai => clientOperationIdsList.Contains(ai.ClientOperationId))
            .Select(ai => new { ai.ClientOperationId, ai.CollectionBankId })
            .ToDictionaryAsync(x => x.ClientOperationId, x => x.CollectionBankId, cancellationToken);
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