using Closing.Integrations.YieldsToDistribute.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.YieldsToDistribute;

public class GetDistributedYieldGroupByConceptConsumer(ISender sender) : IRpcHandler<GetDistributedYieldGroupByConceptRequest, GetDistributedYieldGroupByConceptResponse>
{
    public async Task<GetDistributedYieldGroupByConceptResponse> HandleAsync(GetDistributedYieldGroupByConceptRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetDistributedYieldsGroupedQuery(request.PortfolioIds, request.ClosingDate, request.Concept), ct);
        
        if (!result.IsSuccess)
            return new GetDistributedYieldGroupByConceptResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetDistributedYieldGroupByConceptResponse(true, result.Value, null, null);
    }
}
