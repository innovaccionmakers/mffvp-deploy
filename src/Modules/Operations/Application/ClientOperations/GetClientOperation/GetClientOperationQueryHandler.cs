using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ClientOperations;
using Operations.Integrations.ClientOperations.GetClientOperation;
using Operations.Integrations.ClientOperations;

namespace Operations.Application.ClientOperations.GetClientOperation;

internal sealed class GetClientOperationQueryHandler(
    IClientOperationRepository clientoperationRepository)
    : IQueryHandler<GetClientOperationQuery, ClientOperationResponse>
{
    public async Task<Result<ClientOperationResponse>> Handle(GetClientOperationQuery request,
        CancellationToken cancellationToken)
    {
        var clientoperation = await clientoperationRepository.GetAsync(request.ClientOperationId, cancellationToken);
        if (clientoperation is null)
            return Result.Failure<ClientOperationResponse>(ClientOperationErrors.NotFound(request.ClientOperationId));
        var response = new ClientOperationResponse(
            clientoperation.ClientOperationId,
            clientoperation.Date,
            clientoperation.AffiliateId,
            clientoperation.ObjectiveId,
            clientoperation.PortfolioId,
            clientoperation.Amount,
            clientoperation.SubtransactionTypeId
        );
        return response;
    }
}