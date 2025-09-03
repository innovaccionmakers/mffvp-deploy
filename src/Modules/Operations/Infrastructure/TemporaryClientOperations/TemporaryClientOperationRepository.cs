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

    public async Task<TemporaryClientOperation?> GetForUpdateAsync(long temporaryClientOperationId, CancellationToken cancellationToken = default)
    {
        return await context.TemporaryClientOperations
            .Where(x => x.TemporaryClientOperationId == temporaryClientOperationId && !x.Processed)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TemporaryClientOperation>> GetByPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        return await context.TemporaryClientOperations
            .Where(x => x.PortfolioId == portfolioId && !x.Processed)
            .OrderBy(x => x.RegistrationDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyCollection<TemporaryClientOperation>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        return await context.TemporaryClientOperations
            .Where(x => ids.Contains(x.TemporaryClientOperationId))
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
    
    public void DeleteRange(IEnumerable<TemporaryClientOperation> operations)
    {
        context.TemporaryClientOperations.RemoveRange(operations);
    }

    public async Task<long?> GetNextPendingIdAsync(
    int portfolioId,
    CancellationToken cancellationToken = default)
    {
        return await context.TemporaryClientOperations
            .AsNoTracking()
            .Where(t => t.PortfolioId == portfolioId && !t.Processed)
            .OrderBy(t => t.RegistrationDate)              
            .ThenBy(t => t.TemporaryClientOperationId)      
            .Select(t => (long?)t.TemporaryClientOperationId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> MarkProcessedIfPendingAsync(
    long temporaryClientOperationId,
    CancellationToken cancellationToken = default)
    {
        return context.TemporaryClientOperations
            .Where(x => x.TemporaryClientOperationId == temporaryClientOperationId && !x.Processed)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(x => x.Processed, true)
                    ,
                cancellationToken);
    }

}
