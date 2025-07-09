using Microsoft.EntityFrameworkCore;
using Operations.Domain.TemporaryClientOperations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.TemporaryClientOperations;

internal sealed class TemporaryClientOperationRepository(OperationsDbContext context) : ITemporaryClientOperationRepository
{
    public async Task<IReadOnlyCollection<TemporaryClientOperation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TemporaryClientOperations.ToListAsync(cancellationToken);
    }

    public async Task<TemporaryClientOperation?> GetAsync(long temporaryClientOperationId, CancellationToken cancellationToken = default)
    {
        return await context.TemporaryClientOperations
            .SingleOrDefaultAsync(x => x.TemporaryClientOperationId == temporaryClientOperationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TemporaryClientOperation>> GetByPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        return await context.TemporaryClientOperations
            .Where(x => x.PortfolioId == portfolioId)
            .OrderBy(x => x.RegistrationDate)
            .ToListAsync(cancellationToken);
    }

    public void Insert(TemporaryClientOperation temporaryClientOperation)
    {
        context.TemporaryClientOperations.Add(temporaryClientOperation);
    }

    public void Update(TemporaryClientOperation temporaryClientOperation)
    {
        context.TemporaryClientOperations.Update(temporaryClientOperation);
    }

    public void Delete(TemporaryClientOperation temporaryClientOperation)
    {
        context.TemporaryClientOperations.Remove(temporaryClientOperation);
    }
}
