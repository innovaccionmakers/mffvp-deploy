
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Trusts.Integrations.DataSync.TrustSync.Queries;
using Trusts.Integrations.DataSync.TrustSync.Response;

namespace Trusts.IntegrationEvents.DataSync.TrustSync;
public sealed class ActiveTrustsByPortfolioConsumer
       : IRpcHandler<ActiveTrustsByPortfolioRequest, ActiveTrustsByPortfolioResponse>
{
    private readonly ISender _mediator;

    public ActiveTrustsByPortfolioConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<ActiveTrustsByPortfolioResponse> HandleAsync(
        ActiveTrustsByPortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetActiveTrustsByPortfolioQuery(request.PortfolioId),
            cancellationToken);

        return result.Match(
            success => new ActiveTrustsByPortfolioResponse(
                true,
                null,
                null,
                success),
            error => new ActiveTrustsByPortfolioResponse(
                false,
                error.Code,
                error.Description,
                Array.Empty<GetActiveTrustByPortfolioResponse>()));
    }
}
