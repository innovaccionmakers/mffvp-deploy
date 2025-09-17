using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.Yields;

public sealed class GetYieldsByPortfolioIdsAndClosingDateConsumer(ISender sender) : IRpcHandler<GetYieldsByPortfolioIdsAndClosingDateRequest, GetYieldsByPortfolioIdsAndClosingDateResponse>
{
    public async Task<GetYieldsByPortfolioIdsAndClosingDateResponse> HandleAsync(GetYieldsByPortfolioIdsAndClosingDateRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllReturnsQuery(request.PortfolioIds, request.ClosingDate), ct);

        if(!result.IsSuccess)
            return new GetYieldsByPortfolioIdsAndClosingDateResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetYieldsByPortfolioIdsAndClosingDateResponse(true, result.Value, null, null);
    }
}
