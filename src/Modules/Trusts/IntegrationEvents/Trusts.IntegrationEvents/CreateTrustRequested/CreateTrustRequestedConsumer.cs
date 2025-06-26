using DotNetCore.CAP;
using MediatR;
using Trusts.Integrations.Trusts.CreateTrust;

namespace Trusts.IntegrationEvents.CreateTrustRequested;

public sealed class CreateTrustRequestedConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public CreateTrustRequestedConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(CreateTrustRequestedIntegrationEvent))]
    public async Task HandleAsync(CreateTrustRequestedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CreateTrustCommand(
                message.AffiliateId,
                message.ClientOperationId,
                message.CreationDate,
                message.ObjectiveId,
                message.PortfolioId,
                message.TotalBalance,
                message.TotalUnits,
                message.Principal,
                message.Earnings,
                message.TaxCondition,
                message.ContingentWithholding,
                message.EarningsWithholding,
                message.AvailableAmount),
            cancellationToken);
    }
}