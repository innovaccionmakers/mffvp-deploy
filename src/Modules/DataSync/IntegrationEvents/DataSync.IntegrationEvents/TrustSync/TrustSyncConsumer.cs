using MediatR;
using DataSync.Integrations.TrustSync;
using Common.SharedKernel.Application.Rpc;

namespace DataSync.IntegrationEvents.TrustSync;

public sealed class TrustSyncConsumer(
    ISender mediator) : IRpcHandler<TrustSyncRequest, TrustSyncResponse>
{
    public async Task<TrustSyncResponse> HandleAsync(
        TrustSyncRequest message,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new TrustSyncCommand(message.ClosingDate, message.PortfolioId),
            cancellationToken);

        return result.IsSuccess
            ? new TrustSyncResponse(true)
            : new TrustSyncResponse(false, result.Error!.Code, result.Error.Description);
    }
}
