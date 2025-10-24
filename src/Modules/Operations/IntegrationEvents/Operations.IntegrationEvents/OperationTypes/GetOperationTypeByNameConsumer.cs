using Common.SharedKernel.Application.Rpc;
using MediatR;
using Operations.Integrations.OperationTypes;

namespace Operations.IntegrationEvents.OperationTypes;

public sealed class GetOperationTypeByNameConsumer(ISender mediator) : IRpcHandler<GetOperationTypeByNameRequest, GetOperationTypeByNameResponse>
{
    public async Task<GetOperationTypeByNameResponse> HandleAsync(
        GetOperationTypeByNameRequest message,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOperationTypeByNameQuery(message.Name), cancellationToken);

        return result.Match(
            type => new GetOperationTypeByNameResponse(true, null, null, type),
            err => new GetOperationTypeByNameResponse(false, err.Code, err.Description, Array.Empty<OperationTypeResponse>()));
    }
}
