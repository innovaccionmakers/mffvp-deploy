using Common.SharedKernel.Application.Rpc;
using MediatR;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.IntegrationEvents.ClientOperations;

public sealed class AccountingDebitNoteOperationsConsumer(ISender mediator) : IRpcHandler<GetAccountingDebitNoteOperationsRequestEvents, GetAccountingDebitNoteOperationsValidationResponse>
{
    public async Task<GetAccountingDebitNoteOperationsValidationResponse> HandleAsync(GetAccountingDebitNoteOperationsRequestEvents request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAccountingDebitNoteOperationsQuery(request.PortfolioId, request.ProcessDate),
                cancellationToken);

        return result.Match(
            success => new GetAccountingDebitNoteOperationsValidationResponse(true, null, null, success),
            failure => new GetAccountingDebitNoteOperationsValidationResponse(false, failure.Code, failure.Description, Array.Empty<GetAccountingOperationsResponse>())
        );
    }
}
