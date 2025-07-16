using Common.SharedKernel.Application.Rpc;
using MediatR;
using Operations.Integrations.SubTransactionTypes;

namespace Operations.IntegrationEvents.SubTransactionTypes;

public sealed class GetAllOperationTypesConsumer : IRpcHandler<GetAllOperationTypesRequest, GetAllOperationTypesResponse>
{
    private readonly ISender _mediator;

    public GetAllOperationTypesConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<GetAllOperationTypesResponse> HandleAsync(
        GetAllOperationTypesRequest message,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllOperationTypesQuery(), cancellationToken);

        return result.Match(
            types => new GetAllOperationTypesResponse(true, null, null, types),
            err => new GetAllOperationTypesResponse(false, err.Code, err.Description, Array.Empty<SubtransactionTypeResponse>()));
    }
}