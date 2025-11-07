using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Operations.Domain.TrustOperations;
using Operations.Infrastructure.Database;
using System.Linq;
namespace Operations.Infrastructure.TrustOperations;

internal sealed class TrustOperationRepository(OperationsDbContext context)
    : ITrustOperationRepository
{
    public async Task AddAsync(TrustOperation operation, CancellationToken cancellationToken)
    {
        await context.TrustOperations.AddAsync(operation, cancellationToken);
    }

    public async Task<TrustOperation?> GetForUpdateByPortfolioTrustAndDateAsync(
      int portfolioId,
      long trustId,
      DateTime closingDate,
      CancellationToken cancellationToken)
    {
        return await context.TrustOperations
            .FirstOrDefaultAsync(
                op => op.PortfolioId == portfolioId
                     && op.TrustId == trustId
                   && op.ProcessDate.Date == closingDate.Date,
                cancellationToken);
    }

    public async Task<TrustOperation?> GetByPortfolioAndTrustAsync(
      int portfolioId,
      long trustId,
      DateTime closingDate,
      CancellationToken cancellationToken)
    {
        return await context.TrustOperations.AsNoTracking()
            .FirstOrDefaultAsync(
                op => op.PortfolioId == portfolioId
                   && op.TrustId == trustId
                   && op.ProcessDate.Date == closingDate.Date,
                cancellationToken);
    }

    public void Update(TrustOperation operation)
    {
        context.TrustOperations.Update(operation);
    }

    public async Task<IReadOnlyCollection<TrustOperation>> GetByPortfolioProcessDateAndOperationTypeAsync(
        int portfolioId,
        DateTime processDate,
        long operationTypeId,
        CancellationToken cancellationToken)
    {
        var targetDate = processDate.Date;

        return await context.TrustOperations
            .AsNoTracking()
            .Where(op =>
                op.PortfolioId == portfolioId &&
                op.OperationTypeId == operationTypeId &&
                op.ProcessDate.Date == targetDate)
            .ToListAsync(cancellationToken);
    }


}