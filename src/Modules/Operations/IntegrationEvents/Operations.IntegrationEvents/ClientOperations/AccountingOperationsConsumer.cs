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
            var result = await mediator.Send(new GetAccountingOperationsQuery(request.PortfolioId, request.ProcessDate, request.OperationTypeName, request.ClientOperationTypeName),
                    cancellationToken);

            return result.Match(
                success => new GetAccountingOperationsValidationResponse(true, null, null, success),
                failure => new GetAccountingOperationsValidationResponse(false, failure.Code, failure.Description, Array.Empty<GetAccountingOperationsResponse>())
            );
        }
    }
}
