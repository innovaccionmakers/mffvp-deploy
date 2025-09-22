using Common.SharedKernel.Application.Rpc;
using MediatR;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.IntegrationEvents.ClientOperations
{
    public sealed class AccountingOperationsConsumer(ISender mediator) : IRpcHandler<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>
    {
        public async Task<GetAccountingOperationsValidationResponse> HandleAsync(GetAccountingOperationsRequestEvents request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAccountingOperationsQuery(request.PortfolioId, request.ProcessDate),
                    cancellationToken);

            return result.Match(
                clientOperations => new GetAccountingOperationsValidationResponse(true, null, null, clientOperations),
                err => new GetAccountingOperationsValidationResponse(false, err.Code, err.Description, Array.Empty<GetAccountingOperationsResponse>())
            );
        }
    }
}
