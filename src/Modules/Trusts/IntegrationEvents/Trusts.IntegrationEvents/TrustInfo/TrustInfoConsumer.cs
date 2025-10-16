using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Trusts.Integrations.Trusts.TrustInfo;

namespace Trusts.IntegrationEvents.TrustInfo;

public sealed class TrustInfoConsumer(ISender mediator)
    : IRpcHandler<TrustInfoRequest, TrustInfoResponse>
{
    public async Task<TrustInfoResponse> HandleAsync(
        TrustInfoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new TrustInfoQuery(request.ClientOperationId, request.ContributionValue),
            cancellationToken);

        return result.Match(
            validation => new TrustInfoResponse(true, null, null, validation.TrustId),
            error => new TrustInfoResponse(false, error.Code, error.Description, null));
    }
}
