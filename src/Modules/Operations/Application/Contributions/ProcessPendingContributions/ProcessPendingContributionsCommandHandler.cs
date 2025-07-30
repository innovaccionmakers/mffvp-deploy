using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Abstractions.Data;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Integrations.Contributions.ProcessPendingContributions;
using Trusts.IntegrationEvents.CreateTrustRequested;

namespace Operations.Application.Contributions.ProcessPendingContributions;

internal sealed class ProcessPendingContributionsCommandHandler(
    ITemporaryClientOperationRepository tempOpRepo,
    ITemporaryAuxiliaryInformationRepository tempAuxRepo,
    ITransactionControl transactionControl,
    IEventBus eventBus,
    ITempClientOperationsCleanupService cleanupService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ProcessPendingContributionsCommand>
{
    public async Task<Result> Handle(ProcessPendingContributionsCommand request, CancellationToken cancellationToken)
    {
        var operations = await tempOpRepo.GetByPortfolioAsync(request.PortfolioId, cancellationToken);
        var tempOpIds = new List<long>();
        var tempAuxIds = new List<long>();

        foreach (var temp in operations)
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
            
            temp.MarkAsProcessed();
            tempOpRepo.Update(temp);
            await unitOfWork.SaveChangesAsync(cancellationToken);

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

        if (tempOpIds.Count > 0)
        {
            await cleanupService.ScheduleCleanupAsync(tempOpIds, tempAuxIds, cancellationToken);
        }

        return Result.Success();
    }
}
