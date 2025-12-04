using Closing.Integrations.YieldDetails.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.Yields;

public sealed class GetYieldDetailsByPortfolioIdsAndClosingDateConsumer(ISender sender)
    : IRpcHandler<GetYieldDetailsByPortfolioIdsAndClosingDateRequest, GetYieldDetailsByPortfolioIdsAndClosingDateResponse>
{
    public async Task<GetYieldDetailsByPortfolioIdsAndClosingDateResponse> HandleAsync(
        GetYieldDetailsByPortfolioIdsAndClosingDateRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetYieldDetailsByPortfolioIdsAndClosingDateQuery(
                request.PortfolioIds,
                request.ClosingDate,
                request.Source),
            ct);

        if (!result.IsSuccess)
            return new GetYieldDetailsByPortfolioIdsAndClosingDateResponse(
                false,
                null,
                result.Error.Code,
                result.Error.Description);

        return new GetYieldDetailsByPortfolioIdsAndClosingDateResponse(
            true,
            result.Value,
            null,
            null);
    }
}

