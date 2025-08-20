using MediatR;
using Closing.Integrations.TrustSync;
using Common.SharedKernel.Application.Rpc;

namespace Closing.IntegrationEvents.DataSync.TrustSync;

public sealed class TrustSyncConsumer(
    ISender mediator) : IRpcHandler<TrustSyncRequest, TrustSyncResponse>
{
    public async Task<TrustSyncResponse> HandleAsync(
        TrustSyncRequest message,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new TrustSyncPostCommand(
                message.PortfolioId,
                message.ClosingDate),
            cancellationToken);

        return result.IsSuccess
            ? new TrustSyncResponse(true)
            : new TrustSyncResponse(false, result.Error!.Code, result.Error!.Description);
    }
}
