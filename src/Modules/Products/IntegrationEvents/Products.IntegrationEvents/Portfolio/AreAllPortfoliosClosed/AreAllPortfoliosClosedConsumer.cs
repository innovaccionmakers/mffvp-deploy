using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Portfolios.Queries;

namespace Products.IntegrationEvents.Portfolio.AreAllPortfoliosClosed;

public sealed class AreAllPortfoliosClosedConsumer(ISender sender)
    : IRpcHandler<AreAllPortfoliosClosedRequest, AreAllPortfoliosClosedResponse>
{
    public async Task<AreAllPortfoliosClosedResponse> HandleAsync(
        AreAllPortfoliosClosedRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new AreAllPortfoliosClosedQuery(request.PortfolioIds, request.Date),
            ct);

        if (!result.IsSuccess)
            return new AreAllPortfoliosClosedResponse(
                false,
                false,
                result.Error.Code,
                result.Error.Description);

        return new AreAllPortfoliosClosedResponse(
            true,
            result.Value,
            null,
            null);
    }
}

