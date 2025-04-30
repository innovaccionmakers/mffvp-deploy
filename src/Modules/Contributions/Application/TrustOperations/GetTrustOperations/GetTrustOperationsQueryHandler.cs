using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.TrustOperations;
using Contributions.Integrations.TrustOperations.GetTrustOperations;
using Contributions.Integrations.TrustOperations;
using System.Collections.Generic;
using System.Linq;

namespace Contributions.Application.TrustOperations.GetTrustOperations;

internal sealed class GetTrustOperationsQueryHandler(
    ITrustOperationRepository trustoperationRepository)
    : IQueryHandler<GetTrustOperationsQuery, IReadOnlyCollection<TrustOperationResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrustOperationResponse>>> Handle(GetTrustOperationsQuery request, CancellationToken cancellationToken)
    {
        var entities = await trustoperationRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new TrustOperationResponse(
                e.TrustOperationId,
                e.ClientOperationId,
                e.TrustId,
                e.Amount))
            .ToList();

        return Result.Success<IReadOnlyCollection<TrustOperationResponse>>(response);
    }
}