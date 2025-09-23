using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Portfolios.Queries;

namespace Products.IntegrationEvents.Portfolio.GetPortfolioInformation;

public sealed class GetPortfolioInformationByIdConsumer(ISender sender) : IRpcHandler<GetPortfolioInformationByIdRequest, GetPortfolioInformationByIdResponse>
{
    public async Task<GetPortfolioInformationByIdResponse> HandleAsync(GetPortfolioInformationByIdRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetPortfolioInformationByIdQuery(request.PortfolioId), ct);
        if (!result.IsSuccess)
            return new GetPortfolioInformationByIdResponse(false, null, result.Error.Code, result.Error.Description);
        return new GetPortfolioInformationByIdResponse(true, result.Value, null, null);
    }
}
