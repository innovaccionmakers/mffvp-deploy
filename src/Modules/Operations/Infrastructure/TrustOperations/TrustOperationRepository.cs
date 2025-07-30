using Operations.Domain.TrustOperations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.TrustOperations;

internal sealed class TrustOperationRepository(OperationsDbContext context)
    : ITrustOperationRepository
{
    public async Task AddAsync(TrustOperation operation, CancellationToken cancellationToken)
    {
        await context.TrustOperations.AddAsync(operation, cancellationToken);
    }
}