using MediatR;
using Closing.Integrations.TrustSync;
using Common.SharedKernel.Application.Rpc;

namespace Closing.IntegrationEvents.TrustSync;

public sealed class TrustSyncConsumer(
    ISender mediator) : IRpcHandler<TrustSyncRequest, TrustSyncResponse>
{
    public async Task<TrustSyncResponse> HandleAsync(
        TrustSyncRequest message,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new TrustSyncCommand(
                message.TrustId,
                message.PortfolioId,
                message.ClosingDate,
                message.PreClosingBalance,
                message.Capital,
                message.ContingentWithholding),
            cancellationToken);

        return result.IsSuccess
            ? new TrustSyncResponse(true)
            : new TrustSyncResponse(false, result.Error!.Code, result.Error.Description);
    }
}
