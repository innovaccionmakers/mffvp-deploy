using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.Yields;

public sealed class GetAllComissionsByPortfolioIdsAndClosingDateConsumer(ISender sender) : IRpcHandler<GetAllComissionsByPortfolioIdsAndClosingDateRequest, GetAllComissionsByPortfolioIdsAndClosingDateResponse>
{
    public async Task<GetAllComissionsByPortfolioIdsAndClosingDateResponse> HandleAsync(GetAllComissionsByPortfolioIdsAndClosingDateRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllFeesQuery(request.PortfolioIds, request.ClosingDate), ct);

        if(!result.IsSuccess)
            return new GetAllComissionsByPortfolioIdsAndClosingDateResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetAllComissionsByPortfolioIdsAndClosingDateResponse(true, result.Value, null, null);
    }
}
