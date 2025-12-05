
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Portfolios.Queries;


namespace Products.IntegrationEvents.Portfolio.GetInfoForClosing;

public sealed class GetPortfolioInfoForClosingConsumer : IRpcHandler<GetPortfolioInfoForClosingRequest, GetPortfolioInfoForClosingResponse>
{
    private readonly ISender _mediator;
    public GetPortfolioInfoForClosingConsumer(ISender mediator) => _mediator = mediator;

    public async Task<GetPortfolioInfoForClosingResponse> HandleAsync(GetPortfolioInfoForClosingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioInfoForClosingQuery(request.PortfolioId), cancellationToken);
        if (!result.IsSuccess)
            return new GetPortfolioInfoForClosingResponse(false, 0, result.Error.Code, result.Error.Description);
        return new GetPortfolioInfoForClosingResponse(true, result.Value.AgileWithdrawalPercentageProtectedBalance, null, null);
    }
}
