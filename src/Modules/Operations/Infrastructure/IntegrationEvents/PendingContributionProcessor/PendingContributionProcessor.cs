using Closing.IntegrationEvents.ProcessPendingContributionsRequested;
using Common.SharedKernel.Application.EventBus;
using DotNetCore.CAP;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Domain.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Trusts.IntegrationEvents.CreateTrustRequested;


namespace Operations.IntegrationEvents.PendingContributionProcessor;

public sealed class PendingContributionProcessor(
    ITemporaryClientOperationRepository tempOpRepo,
    ITemporaryAuxiliaryInformationRepository tempAuxRepo,
    ITransactionControl transactionControl,
    IEventBus eventBus,
    ITempClientOperationsCleanupService cleanupService) : ICapSubscribe
{
    [CapSubscribe(nameof(ProcessPendingContributionsRequestedIntegrationEvent))]
    public async Task HandleAsync(ProcessPendingContributionsRequestedIntegrationEvent message, CancellationToken cancellationToken)
    {
        var byPortfolio = await tempOpRepo.GetByPortfolioAsync(message.PortfolioId, cancellationToken);
        var tempOpIds = new List<long>();
        var tempAuxIds = new List<long>();

        foreach (var temp in byPortfolio)
        {
            var aux = await tempAuxRepo.GetAsync(temp.TemporaryClientOperationId, cancellationToken);
            if (aux is null) continue;

            var op = ClientOperation.Create(
                temp.RegistrationDate,
                temp.AffiliateId,
                temp.ObjectiveId,
                temp.PortfolioId,
                temp.Amount,
                temp.ProcessDate,
                temp.SubtransactionTypeId,
                temp.ApplicationDate).Value;

            var info = AuxiliaryInformation.Create(
                op.ClientOperationId,
                aux.OriginId,
                aux.CollectionMethodId,
                aux.PaymentMethodId,
                aux.CollectionAccount,
                aux.PaymentMethodDetail,
                aux.CertificationStatusId,
                aux.TaxConditionId,
                aux.ContingentWithholding,
                aux.VerifiableMedium,
                aux.CollectionBankId,
                aux.DepositDate,
                aux.SalesUser,
                aux.OriginModalityId,
                aux.CityId,
                aux.ChannelId,
                aux.UserId).Value;

            await transactionControl.ExecuteAsync(op, info, cancellationToken);

            var trustEvent = new CreateTrustRequestedIntegrationEvent(
                op.AffiliateId,
                op.ClientOperationId,
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                op.ObjectiveId,
                op.PortfolioId,
                op.Amount,
                0,
                op.Amount,
                0m,
                aux.TaxConditionId,
                aux.ContingentWithholding,
                0m,
                op.Amount,
                0m,
                true);
            await eventBus.PublishAsync(trustEvent, cancellationToken);

            tempOpIds.Add(temp.TemporaryClientOperationId);
            tempAuxIds.Add(aux.TemporaryAuxiliaryInformationId);
        }

        if (tempOpIds.Count == 0) return;

        await cleanupService.ScheduleCleanupAsync(tempOpIds, tempAuxIds, cancellationToken);
    }
}