using Closing.Integrations.YieldDetails.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.Yields;

public sealed class GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptConsumer(ISender sender)
    : IRpcHandler<GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptRequest, GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptResponse>
{
    public async Task<GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptResponse> HandleAsync(
        GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQuery(
                request.PortfolioIds,
                request.ClosingDate,
                request.Source,
                request.GuidConcept),
            ct);

        if (!result.IsSuccess)
            return new GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptResponse(
                false,
                null,
                result.Error.Code,
                result.Error.Description);

        return new GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptResponse(
            true,
            result.Value,
            null,
            null);
    }
}

