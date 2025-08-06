using Closing.IntegrationEvents.PostClosing;
using DotNetCore.CAP;
using MediatR;
using Products.Integrations.PortfolioValuation.Commands;

namespace Products.IntegrationEvents.PortfolioValuation.PortfolioValuationUpdated;

public sealed class PortfolioUpdatedSuscriber(ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(PortfolioUpdatedIntegrationEvent))]
    public async Task HandleAsync(PortfolioUpdatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpsertPortfolioValuationCommand(
            message.PortfolioId,
            message.ClosingDate
        ), cancellationToken);
    }
}