using Closing.IntegrationEvents.PostClosing;
using DotNetCore.CAP;
using MediatR;
using Products.Integrations.AccumulatedCommissions.Commands;

namespace Products.IntegrationEvents.AccumulatedCommissions.CommissionProcessed;

public sealed class CommissionProcessedSuscriber : ICapSubscribe
{
    private readonly ISender _mediator;

    public CommissionProcessedSuscriber(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(CommissionProcessedIntegrationEvent))]
    public async Task HandleAsync(
        CommissionProcessedIntegrationEvent message,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpsertAccumulatedCommissionCommand(
            message.PortfolioId,
            message.CommissionId,
            message.AccumulatedValue,
            message.ClosingDate
        ), cancellationToken);
    }
}