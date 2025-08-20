
using Closing.Integrations.TrustSync;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.DataSync.TrustSync;

public sealed class TrustSyncPostConsumer(
    ISender mediator) : IRpcHandler<TrustSyncPostRequest, TrustSyncPostResponse>
{
    public async Task<TrustSyncPostResponse> HandleAsync(
        TrustSyncPostRequest message,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new TrustSyncPostCommand(
                message.PortfolioId,
                message.ClosingDate)
            ,cancellationToken);

        return result.IsSuccess
            ? new TrustSyncPostResponse(true)
            : new TrustSyncPostResponse(false, result.Error!.Code, result.Error.Description);
    }
}
