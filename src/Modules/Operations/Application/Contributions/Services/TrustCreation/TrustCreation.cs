using Common.SharedKernel.Application.EventBus;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.TrustCreation;
using Operations.Domain.ClientOperations;
using Operations.Integrations.Contributions.CreateContribution;
using Trusts.IntegrationEvents.CreateTrustRequested;

namespace Operations.Application.Contributions.Services.TrustCreation;

public sealed class TrustCreation(IEventBus eventBus) : ITrustCreation
{
    public async Task ExecuteAsync(
        CreateContributionCommand command,
        ClientOperation clientOperation,
        TaxResult taxResult,
        CancellationToken cancellationToken)
    {
        var createTrustEvent = new CreateTrustRequestedIntegrationEvent(
            clientOperation.AffiliateId,
            clientOperation.ClientOperationId,
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            clientOperation.ObjectiveId,
            clientOperation.PortfolioId,
            command.Amount,
            0m,
            command.Amount,
            0m,
            taxResult.TaxConditionId,
            taxResult.WithheldAmount,
            0m,
            0m,
            true);

        await eventBus.PublishAsync(createTrustEvent, cancellationToken);
    }
} 