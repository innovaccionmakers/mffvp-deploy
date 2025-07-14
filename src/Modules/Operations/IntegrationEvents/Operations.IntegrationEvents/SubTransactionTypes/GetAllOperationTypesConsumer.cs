using Common.SharedKernel.Application.Messaging;
using DotNetCore.CAP;
using MediatR;
using Operations.Integrations.SubTransactionTypes;

namespace Operations.IntegrationEvents.SubTransactionTypes;

public sealed class GetAllOperationTypesConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public GetAllOperationTypesConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(GetAllOperationTypesRequest))]
    public async Task<GetAllOperationTypesResponse> HandleAsync(
        GetAllOperationTypesRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result = await _mediator.Send(new GetAllOperationTypesQuery(), cancellationToken);

        return result.Match(
            types => new GetAllOperationTypesResponse(true, null, null, types),
            err => new GetAllOperationTypesResponse(false, err.Code, err.Description, Array.Empty<SubtransactionTypeResponse>()));
    }
}