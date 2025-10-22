using DotNetCore.CAP;
using MediatR;
using Closing.Integrations.ClientOperations.PreClosingTx;

namespace Closing.IntegrationEvents.DataSync.PreClosingTxRequested;

public sealed class PreClosingTxRequestedConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public PreClosingTxRequestedConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(PreClosingTxRequestedIntegrationEvent))]
    public async Task HandleAsync(PreClosingTxRequestedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new PreClosingTxCommand(
                message.ClientOperationId,
                message.FilingDate,
                message.AffiliateId,
                message.ObjectiveId,
                message.PortfolioId,
                message.Amount,
                message.ProcessDate,
                message.TransactionSubtypeId,
                message.ApplicationDate,
                message.Status,
                message.CauseId,
                message.TrustId,
                message.LinkedClientOperationId,
                message.Units),
            cancellationToken);
    }
}
