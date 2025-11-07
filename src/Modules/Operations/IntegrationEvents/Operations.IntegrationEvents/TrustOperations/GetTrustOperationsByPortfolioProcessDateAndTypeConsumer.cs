using Common.SharedKernel.Application.Rpc;
using MediatR;
using Operations.Integrations.TrustOperations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed class GetTrustOperationsByPortfolioProcessDateAndTypeConsumer(ISender mediator)
    : IRpcHandler<GetTrustOperationsByPortfolioProcessDateAndTypeRequest, GetTrustOperationsByPortfolioProcessDateAndTypeResponse>
{
    public async Task<GetTrustOperationsByPortfolioProcessDateAndTypeResponse> HandleAsync(
        GetTrustOperationsByPortfolioProcessDateAndTypeRequest message,
        CancellationToken cancellationToken)
    {
        var query = new GetTrustOperationsByPortfolioProcessDateAndTypeQuery(
            message.PortfolioId,
            message.ProcessDate,
            message.OperationTypeId);

        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            operations => new GetTrustOperationsByPortfolioProcessDateAndTypeResponse(true, null, null, operations),
            error => new GetTrustOperationsByPortfolioProcessDateAndTypeResponse(false, error.Code, error.Description, Array.Empty<TrustOperationResponse>()));
    }
}
