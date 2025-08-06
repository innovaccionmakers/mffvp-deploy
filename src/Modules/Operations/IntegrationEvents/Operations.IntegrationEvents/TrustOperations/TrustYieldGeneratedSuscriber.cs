
using Closing.IntegrationEvents.PostClosing;
using DotNetCore.CAP;
using MediatR;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed class TrustYieldGeneratedSuscriber(ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(TrustYieldGeneratedIntegrationEvent))]
    public async Task HandleAsync(TrustYieldGeneratedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpsertTrustOperationCommand(
            TrustId: message.TrustId,
            PortfolioId: message.PortfolioId,
            Amount: message.YieldAmount,
            ClosingDate: message.ClosingDate,
            ProcessDate: message.ProcessDate,
            YieldRetention: message.YieldRetention,
            ClosingBalance: message.ClosingBalance,
            Units: message.Units
        ), cancellationToken);
    }
}