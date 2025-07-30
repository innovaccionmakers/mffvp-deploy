
using Closing.IntegrationEvents.PostClosing;
using DotNetCore.CAP;
using MediatR;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed class TrustYieldGeneratedConsumer(ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(TrustYieldGeneratedIntegrationEvent))]
    public async Task HandleAsync(TrustYieldGeneratedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await mediator.Send(new CreateTrustOperationCommand(
            TrustId: message.TrustId,
            PortfolioId: message.PortfolioId,
            Amount: message.YieldAmount,
            ClosingDate: message.ClosingDate,
            ProcessDate: message.ProcessDate
        ), cancellationToken);
    }
}