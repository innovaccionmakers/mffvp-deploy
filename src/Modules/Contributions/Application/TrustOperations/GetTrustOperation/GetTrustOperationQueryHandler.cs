using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.TrustOperations;
using Contributions.Integrations.TrustOperations.GetTrustOperation;
using Contributions.Integrations.TrustOperations;

namespace Contributions.Application.TrustOperations.GetTrustOperation;

internal sealed class GetTrustOperationQueryHandler(
    ITrustOperationRepository trustoperationRepository)
    : IQueryHandler<GetTrustOperationQuery, TrustOperationResponse>
{
    public async Task<Result<TrustOperationResponse>> Handle(GetTrustOperationQuery request, CancellationToken cancellationToken)
    {
        var trustoperation = await trustoperationRepository.GetAsync(request.TrustOperationId, cancellationToken);
        if (trustoperation is null)
        {
            return Result.Failure<TrustOperationResponse>(TrustOperationErrors.NotFound(request.TrustOperationId));
        }
        var response = new TrustOperationResponse(
            trustoperation.TrustOperationId,
            trustoperation.ClientOperationId,
            trustoperation.TrustId,
            trustoperation.Amount
        );
        return response;
    }
}