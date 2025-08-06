using DotNetCore.CAP;
using MediatR;
using Operations.IntegrationEvents.TrustOperations;
using Trusts.Integrations.TrustYields.Commands;

namespace Trusts.IntegrationEvents.TrustYields;

public sealed class TrustYieldOperationAppliedSuscriber(ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(TrustYieldOperationAppliedIntegrationEvent))]
    public async Task HandleAsync(TrustYieldOperationAppliedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateTrustFromYieldCommand(
            TrustId: message.TrustId,
            PortfolioId: message.PortfolioId,
            ClosingDate: message.ClosingDate,
            YieldAmount: message.YieldAmount,
            YieldRetention: message.YieldRetention,
            ClosingBalance: message.ClosingBalance,
            Units: message.Units
        ), cancellationToken);
    }
}