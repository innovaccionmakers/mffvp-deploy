using Closing.Integrations.PortfolioValuations.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.PortfolioValuation;

public class CheckPortfolioValuationExistsConsumer(ISender sender) : IRpcHandler<CheckPortfolioValuationExistsRequest, CheckPortfolioValuationExistsResponse>
{
    public async  Task<CheckPortfolioValuationExistsResponse> HandleAsync(CheckPortfolioValuationExistsRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CheckValuationExistsQuery(request.ClosingDate), ct);

        if (!result.IsSuccess)
            return new CheckPortfolioValuationExistsResponse(false, false, result.Error.Code, result.Error.Description);

        return new CheckPortfolioValuationExistsResponse(true, result.Value.Exists, null, null);
    }
}
