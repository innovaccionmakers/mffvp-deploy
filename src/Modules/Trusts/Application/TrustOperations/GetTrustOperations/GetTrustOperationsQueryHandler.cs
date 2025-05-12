using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.TrustOperations;
using Trusts.Integrations.TrustOperations;
using Trusts.Integrations.TrustOperations.GetTrustOperations;

namespace Trusts.Application.TrustOperations.GetTrustOperations;

internal sealed class GetTrustOperationsQueryHandler(
    ITrustOperationRepository trustoperationRepository)
    : IQueryHandler<GetTrustOperationsQuery, IReadOnlyCollection<TrustOperationResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrustOperationResponse>>> Handle(GetTrustOperationsQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await trustoperationRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new TrustOperationResponse(
                e.TrustOperationId,
                e.CustomerDealId,
                e.TrustId,
                e.Amount))
            .ToList();

        return Result.Success<IReadOnlyCollection<TrustOperationResponse>>(response);
    }
}