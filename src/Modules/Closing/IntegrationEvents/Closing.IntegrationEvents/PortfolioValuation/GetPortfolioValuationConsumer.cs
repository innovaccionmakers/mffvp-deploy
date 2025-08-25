using Closing.Integrations.PortfolioValuation;
using Closing.Integrations.PortfolioValuations.Queries;
using Closing.Integrations.PortfolioValuations.Response;
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.IntegrationEvents.PortfolioValuation;

namespace Closing.IntegrationEvents.PortfolioValuation;

public class GetPortfolioValuationConsumer(ISender sender) : IRpcHandler<GetPortfolioValuationRequest, GetPortfolioValuationResponse>
{
    public async Task<GetPortfolioValuationResponse> HandleAsync(GetPortfolioValuationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetPortfolioValuationQuery(request.ClosingDate), ct);

        if(!result.IsSuccess)
            return new GetPortfolioValuationResponse(false, result.Error.Code, result.Error.Description, []);

        return new GetPortfolioValuationResponse(true, null, null, result.Value);
    }
}
