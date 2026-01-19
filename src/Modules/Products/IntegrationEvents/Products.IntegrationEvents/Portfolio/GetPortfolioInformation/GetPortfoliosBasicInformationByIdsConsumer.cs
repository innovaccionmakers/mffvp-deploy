using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Portfolios.Queries;

namespace Products.IntegrationEvents.Portfolio.GetPortfolioInformation;

public sealed class GetPortfoliosBasicInformationByIdsConsumer(ISender sender)
    : IRpcHandler<GetPortfoliosBasicInformationByIdsRequest, GetPortfoliosBasicInformationByIdsResponse>
{
    public async Task<GetPortfoliosBasicInformationByIdsResponse> HandleAsync(
        GetPortfoliosBasicInformationByIdsRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPortfoliosBasicInformationByIdsQuery(request.PortfolioIds), ct);
        if (!result.IsSuccess)
            return new GetPortfoliosBasicInformationByIdsResponse(false, Array.Empty<PortfolioBasicInformationResponse>(), result.Error.Code, result.Error.Description);

        var portfolios = result.Value.Select(p => new PortfolioBasicInformationResponse(
            p.PortfolioId,
            p.NitApprovedPortfolio,
            p.VerificationDigit,
            p.Name
        )).ToList();

        return new GetPortfoliosBasicInformationByIdsResponse(true, portfolios, null, null);
    }
}

