using Common.SharedKernel.Application.Rpc;
using MediatR;
using Trusts.Integrations.Trusts.GetBalances;
using Common.SharedKernel.Domain;

namespace Trusts.IntegrationEvents.GetBalances;

public sealed class GetBalancesConsumer : IRpcHandler<GetBalancesRequest, GetBalancesResponse>
{
    private readonly ISender _mediator;

    public GetBalancesConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<GetBalancesResponse> HandleAsync(
        GetBalancesRequest message,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBalancesQuery(message.AffiliateId), cancellationToken);

        return result.Match(
            balances => new GetBalancesResponse(true, null, null, balances),
            err => new GetBalancesResponse(false, err.Code, err.Description, Array.Empty<BalanceResponse>()));
    }
}
