using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Portfolios.GetPortfolio;

namespace Products.IntegrationEvents.Portfolio;

public class GetPortfolioByIdConsumer(ISender sender) : IRpcHandler<GetPortfolioByIdRequest, GetPortfolioByIdResponse>
{
    public async Task<GetPortfolioByIdResponse> HandleAsync(GetPortfolioByIdRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetPortfolioQuery(request.PortfolioId), ct);
        if (!result.IsSuccess)
            return new GetPortfolioByIdResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetPortfolioByIdResponse(true, result.Value, null, null);
    }
}
