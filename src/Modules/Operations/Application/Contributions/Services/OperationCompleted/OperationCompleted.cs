using Closing.IntegrationEvents.CreateClientOperationRequested;
using Common.SharedKernel.Application.EventBus;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Domain.ClientOperations;

namespace Operations.Application.Contributions.Services.OperationCompleted;

public sealed class OperationCompleted(IEventBus eventBus) : IOperationCompleted
{
    public async Task ExecuteAsync(ClientOperation operation, CancellationToken cancellationToken)
    {
        var createClosingEvent = new CreateClientOperationRequestedIntegrationEvent(
            operation.ClientOperationId,
            operation.RegistrationDate,
            operation.AffiliateId,
            operation.ObjectiveId,
            operation.PortfolioId,
            operation.Amount,
            operation.ProcessDate,
            operation.SubtransactionTypeId,
            operation.ApplicationDate);

        await eventBus.PublishAsync(createClosingEvent, cancellationToken);
    }
}