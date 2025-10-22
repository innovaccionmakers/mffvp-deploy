using Closing.IntegrationEvents.DataSync.PreClosingTxRequested;
using Common.SharedKernel.Application.EventBus;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Domain.ClientOperations;

namespace Operations.Application.Contributions.Services.OperationCompleted;

public sealed class OperationCompleted(IEventBus eventBus) : IOperationCompleted
{
    public async Task ExecuteAsync(ClientOperation operation, CancellationToken cancellationToken)
    {
        var preClosingEvent = new PreClosingTxRequestedIntegrationEvent(
            operation.ClientOperationId,
            operation.RegistrationDate,
            operation.AffiliateId,
            operation.ObjectiveId,
            operation.PortfolioId,
            operation.Amount,
            operation.ProcessDate,
            operation.OperationTypeId,
            operation.ApplicationDate,
            operation.Status,
            operation.CauseId,
            operation.TrustId,
            operation.LinkedClientOperationId,
            operation.Units);

        await eventBus.PublishAsync(preClosingEvent, cancellationToken);
    }
}
