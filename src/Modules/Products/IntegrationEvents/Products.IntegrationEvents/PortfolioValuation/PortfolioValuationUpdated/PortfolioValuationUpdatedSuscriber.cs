using Closing.IntegrationEvents.PostClosing;
using DotNetCore.CAP;
using MediatR;
using Products.Integrations.PortfolioValuation.Commands;

namespace Products.IntegrationEvents.PortfolioValuation.PortfolioValuationUpdated;

public sealed class PortfolioValuationUpdatedSuscriber(ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(PortfolioValuationUpdatedIntegrationEvent))]
    public async Task HandleAsync(PortfolioValuationUpdatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpsertPortfolioValuationCommand(
            message.PortfolioId,
            message.ClosingDate,
            message.Amount,
            message.Units,
            message.UnitValue,
            message.GrossYieldPerUnit,
            message.CostPerUnit,
            message.DailyProfitability,
            message.IncomingOperations,
            message.OutgoingOperations,
            message.ProcessDate
        ), cancellationToken);
    }
}