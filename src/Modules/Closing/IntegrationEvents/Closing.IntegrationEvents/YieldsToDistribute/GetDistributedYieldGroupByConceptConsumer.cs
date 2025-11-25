using Closing.Integrations.YieldsToDistribute.Queries;
using MediatR;

namespace Closing.IntegrationEvents.YieldsToDistribute;

internal class GetDistributedYieldGroupByConceptConsumer(ISender sender)
{
    public async Task<GetDistributedYieldGroupByConceptResponse> HandleAsync(GetDistributedYieldGroupByConceptRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetDistributedYieldsGroupedQuery(request.PortfolioIds, request.ClosingDate, request.Concept), ct);
        
        if (!result.IsSuccess)
            return new GetDistributedYieldGroupByConceptResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetDistributedYieldGroupByConceptResponse(true, result.Value, null, null);
    }
}
