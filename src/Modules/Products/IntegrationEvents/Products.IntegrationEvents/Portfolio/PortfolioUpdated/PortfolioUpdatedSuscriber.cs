using Closing.IntegrationEvents.PostClosing;
using DotNetCore.CAP;
using MediatR;
using Products.Integrations.Portfolios.Commands;

namespace Products.IntegrationEvents.Portfolio.PortfolioUpdated;

/// <summary>
/// Consume <see cref="PortfolioUpdatedIntegrationEvent"/> y actualiza en el dominio Product
/// la fecha actual del portafolio tras el cierre.
/// </summary>

public sealed class PortfolioUpdatedSuscriber(ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(PortfolioUpdatedIntegrationEvent))]
    public async Task HandleAsync(PortfolioUpdatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdatePortfolioFromClosingCommand(
            message.PortfolioId,
            message.ClosingDate
        ), cancellationToken);
    }
}