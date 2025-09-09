using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Portfolios.GetPortfolioById;
using Products.Integrations.Portfolios.Queries;

namespace Products.IntegrationEvents.Portfolio;

public class GetPortfolioByHomologatedCodeConsumer(ISender sender) : IRpcHandler<GetPortfolioByHomologatedCodeRequest, GetPortfolioByHomologatedCodeResponse>
{
    public async Task<GetPortfolioByHomologatedCodeResponse> HandleAsync(GetPortfolioByHomologatedCodeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetPortfolioByHomologatedCodeQuery(request.HomologatedCode), ct);

        if (!result.IsSuccess)
            return new GetPortfolioByHomologatedCodeResponse(false, null, result.Error.Code, result.Error.Description);

        return new GetPortfolioByHomologatedCodeResponse(true, result.Value, null, null);
    }
}
