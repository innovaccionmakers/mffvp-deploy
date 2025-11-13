using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Core.Primitives;

namespace Closing.Domain.ClientOperations;

public interface IClientOperationRepository
{
    void Insert(ClientOperation clientOperation);

    void Update(ClientOperation clientOperation);

    Task<bool> ClientOperationsExistsAsync(int portfolioId, DateTime closingDateUtc, long transactionSubtypeId, CancellationToken cancellationToken = default);
    Task<decimal> SumByPortfolioAndSubtypesAsync(
       int portfolioId,
       DateTime closingDateUtc,
       IEnumerable<long> subtransactionTypeIds,
       IEnumerable<LifecycleStatus> allowedStatuses,
       CancellationToken cancellationToken = default);

    Task<ClientOperation?> GetForUpdateByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<long>> GetTrustIdsByOperationTypeAndProcessDateAsync(
        IEnumerable<long> trustIds,
        DateTime processDateUtc,
        long operationTypeId,
        CancellationToken cancellationToken = default);
}
