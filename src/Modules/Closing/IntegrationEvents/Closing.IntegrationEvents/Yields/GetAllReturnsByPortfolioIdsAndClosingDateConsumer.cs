using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.Yields;

public sealed class GetAllReturnsByPortfolioIdsAndClosingDateConsumer(ISender sender) : IRpcHandler<GetAllReturnsByPortfolioIdsAndClosingDateRequest, GetAllReturnsByPortfolioIdsAndClosingDateResponse>
{
    public async Task<GetAllReturnsByPortfolioIdsAndClosingDateResponse> HandleAsync(GetAllReturnsByPortfolioIdsAndClosingDateRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllReturnsQuery(request.PortfolioIds, request.ClosingDate), ct);

        if (!result.IsSuccess)
            return new GetAllReturnsByPortfolioIdsAndClosingDateResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetAllReturnsByPortfolioIdsAndClosingDateResponse(true, result.Value, null, null);
    }
}
