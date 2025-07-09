using Closing.IntegrationEvents.ProcessPendingContributionsRequested;
using Closing.IntegrationEvents.CreateClientOperationRequested;
using Common.SharedKernel.Application.EventBus;
using DotNetCore.CAP;
using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Trusts.IntegrationEvents.CreateTrustRequested;


namespace Operations.IntegrationEvents.PendingContributionProcessor;

public sealed class PendingContributionProcessor(
    ITemporaryClientOperationRepository tempOpRepo,
    ITemporaryAuxiliaryInformationRepository tempAuxRepo,
    IClientOperationRepository clientOpRepo,
    IAuxiliaryInformationRepository auxRepo,
    IEventBus eventBus,
    IUnitOfWork unitOfWork) : ICapSubscribe
{
    [CapSubscribe(nameof(ProcessPendingContributionsRequestedIntegrationEvent))]
    public async Task HandleAsync(ProcessPendingContributionsRequestedIntegrationEvent message, CancellationToken cancellationToken)
    {
        var temps = await tempOpRepo.GetAllAsync(cancellationToken);
        var byPortfolio = temps.Where(t => t.PortfolioId == message.PortfolioId)
            .OrderBy(t => t.RegistrationDate)
            .ToList();

        foreach (var temp in byPortfolio)
        {
            var aux = await tempAuxRepo.GetAsync(temp.TemporaryClientOperationId, cancellationToken);
            if (aux is null) continue;

            await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var op = ClientOperation.Create(
                temp.RegistrationDate,
                temp.AffiliateId,
                temp.ObjectiveId,
                temp.PortfolioId,
                temp.Amount,
                temp.ProcessDate,
                temp.SubtransactionTypeId,
                temp.ApplicationDate).Value;
            clientOpRepo.Insert(op);
            await unitOfWork.SaveChangesAsync(cancellationToken);

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
            auxRepo.Insert(info);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var evt = new CreateClientOperationRequestedIntegrationEvent(
                op.ClientOperationId,
                op.RegistrationDate,
                op.AffiliateId,
                op.ObjectiveId,
                op.PortfolioId,
                op.Amount,
                op.ProcessDate,
                op.SubtransactionTypeId,
                op.ApplicationDate);
            await eventBus.PublishAsync(evt, cancellationToken);
            
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

            tempAuxRepo.Delete(aux);
            tempOpRepo.Delete(temp);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
        }
    }
}
