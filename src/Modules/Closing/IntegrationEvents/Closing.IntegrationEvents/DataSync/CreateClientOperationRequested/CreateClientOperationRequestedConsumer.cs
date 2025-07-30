using DotNetCore.CAP;
using MediatR;
using Closing.Integrations.ClientOperations.CreateClientOperation;

namespace Closing.IntegrationEvents.DataSync.CreateClientOperationRequested;

public sealed class CreateClientOperationRequestedConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public CreateClientOperationRequestedConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(CreateClientOperationRequestedIntegrationEvent))]
    public async Task HandleAsync(CreateClientOperationRequestedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CreateClientOperationCommand(
                message.ClientOperationId,
                message.FilingDate,
                message.AffiliateId,
                message.ObjectiveId,
                message.PortfolioId,
                message.Amount,
                message.ProcessDate,
                message.TransactionSubtypeId,
                message.ApplicationDate),
            cancellationToken);
    }
} 