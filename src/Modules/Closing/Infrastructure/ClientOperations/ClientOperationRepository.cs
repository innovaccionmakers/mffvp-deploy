using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Closing.Domain.ClientOperations;
using Closing.Infrastructure.Database;
using Common.SharedKernel.Core.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.ClientOperations;

internal sealed class ClientOperationRepository(ClosingDbContext context) : IClientOperationRepository
{
    public void Insert(ClientOperation clientOperation)
    {
        context.ClientOperations.Add(clientOperation);
    }

    public void Update(ClientOperation clientOperation)
    {
        context.ClientOperations.Update(clientOperation);
    }
    public async Task<bool> ClientOperationsExistsAsync(int portfolioId, DateTime closingDateUtc, long operationTypeId, CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations.AsNoTracking().TagWith("ClientOperationRepository_ClientOperationsExistsAsync")
            .AnyAsync(co => co.Status == LifecycleStatus.Active
                            && co.PortfolioId == portfolioId
                            && co.ProcessDate == closingDateUtc
                            && co.OperationTypeId == operationTypeId,
                cancellationToken);
    }

    public async Task<decimal> SumByPortfolioAndSubtypesAsync(
       int portfolioId,
       DateTime closingDateUtc,
       IEnumerable<long> subtransactionTypeIds,
       IEnumerable<LifecycleStatus> allowedStatuses,
       CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations.AsNoTracking().TagWith("ClientOperationRepository_SumByPortfolioAndSubtypesAsync")
            .Where(op => allowedStatuses.Contains(op.Status)
                         && op.PortfolioId == portfolioId
                         && op.ProcessDate.Date == closingDateUtc.Date
                         && subtransactionTypeIds.Contains(op.OperationTypeId))
            .SumAsync(op => (decimal?)op.Amount, cancellationToken) ?? 0m;
    }
    
    public async Task<ClientOperation?> GetForUpdateByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations
            .Where(co => co.ClientOperationId == id)
            .TagWith("ClientOperationRepository_GetForUpdateByIdAsync")
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<long>> GetTrustIdsByOperationTypeAndProcessDateAsync(
        IEnumerable<long> trustIds,
        DateTime processDateUtc,
        long operationTypeId,
        CancellationToken cancellationToken = default)
    {
        var ids = trustIds?.Distinct().ToArray() ?? Array.Empty<long>();
        if (ids.Length == 0 || operationTypeId <= 0)
        {
            return Array.Empty<long>();
        }

        var matches = await context.ClientOperations
            .AsNoTracking()
            .TagWith("ClientOperationRepository_GetTrustIdsByOperationTypeAndProcessDateAsync")
            .Where(co =>
                co.ProcessDate == processDateUtc &&
                co.OperationTypeId == operationTypeId &&
                co.TrustId != null &&
                ids.Contains(co.TrustId.Value))
            .Select(co => co.TrustId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return matches;
    }
}