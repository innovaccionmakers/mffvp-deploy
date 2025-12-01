using Closing.Integrations.YieldDetails.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.Yields;

public sealed class GetYieldsDetailsByPortfolioIdsClosingDateAndSourceConsumer(ISender sender) : IRpcHandler<GetYieldsDetailsByPortfolioIdsClosingDateAndSourceRequest, GetYieldsDetailsByPortfolioIdsClosingDateAndSourceResponse>
{
    public async Task<GetYieldsDetailsByPortfolioIdsClosingDateAndSourceResponse> HandleAsync(GetYieldsDetailsByPortfolioIdsClosingDateAndSourceRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetYieldDetailsByPortfolioIdsAndClosingDateQuery(request.PortfolioIds, request.ClosingDate, request.Source, request.Concept), ct);

        if (!result.IsSuccess)
            return new GetYieldsDetailsByPortfolioIdsClosingDateAndSourceResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetYieldsDetailsByPortfolioIdsClosingDateAndSourceResponse(true, result.Value, null, null);
    }
}

