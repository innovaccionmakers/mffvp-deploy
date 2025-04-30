using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.ClientOperations;
using Contributions.Integrations.ClientOperations.GetClientOperation;
using Contributions.Integrations.ClientOperations;

namespace Contributions.Application.ClientOperations.GetClientOperation;

internal sealed class GetClientOperationQueryHandler(
    IClientOperationRepository clientoperationRepository)
    : IQueryHandler<GetClientOperationQuery, ClientOperationResponse>
{
    public async Task<Result<ClientOperationResponse>> Handle(GetClientOperationQuery request, CancellationToken cancellationToken)
    {
        var clientoperation = await clientoperationRepository.GetAsync(request.ClientOperationId, cancellationToken);
        if (clientoperation is null)
        {
            return Result.Failure<ClientOperationResponse>(ClientOperationErrors.NotFound(request.ClientOperationId));
        }
        var response = new ClientOperationResponse(
            clientoperation.ClientOperationId,
            clientoperation.Date,
            clientoperation.AffiliateId,
            clientoperation.ObjectiveId,
            clientoperation.PortfolioId,
            clientoperation.TransactionTypeId,
            clientoperation.SubTransactionTypeId,
            clientoperation.Amount
        );
        return response;
    }
}