using MediatR;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Closing.Integrations.TrustSync;
using Common.SharedKernel.Application.Messaging;

namespace Closing.IntegrationEvents.TrustSync;

public sealed class TrustSyncConsumer(
    ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(TrustSyncRequest))]
    public async Task<TrustSyncResponse> HandleAsync(
        TrustSyncRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

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
