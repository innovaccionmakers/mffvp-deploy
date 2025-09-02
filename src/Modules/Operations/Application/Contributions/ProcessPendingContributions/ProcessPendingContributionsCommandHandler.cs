using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.TransactionControl;
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
    IUnitOfWork unitOfWork,
    ILogger<ProcessPendingContributionsCommandHandler> logger)
    : ICommandHandler<ProcessPendingContributionsCommand>
{
    public async Task<Result> Handle(ProcessPendingContributionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var operations = await tempOpRepo.GetByPortfolioAsync(request.PortfolioId, cancellationToken);
            var tempOpIds = new List<long>();
            var tempAuxIds = new List<long>();

            foreach (var temp in operations)
            {
                await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var current = await tempOpRepo.GetForUpdateAsync(temp.TemporaryClientOperationId, cancellationToken);
                    if (current is null)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        continue;
                    }

                    var aux = await tempAuxRepo.GetAsync(current.TemporaryClientOperationId, cancellationToken);
                    if (aux is null)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        continue;
                    }

                    var op = ClientOperation.Create(
                        current.RegistrationDate,
                        current.AffiliateId,
                        current.ObjectiveId,
                        current.PortfolioId,
                        current.Amount,
                        current.ProcessDate,
                        current.OperationTypeId,
                        DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)).Value;

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
                    logger.LogInformation(
                        "Operación Cliente procesada {ClientOperationId} para portafolio {PortfolioId}",
                        op.ClientOperationId,
                        op.PortfolioId);

                    current.MarkAsProcessed();
                    tempOpRepo.Update(current);
                    logger.LogInformation(
                        "Operación temporal {TemporaryClientOperationId} marcada como procesada",
                        current.TemporaryClientOperationId);

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                    logger.LogInformation(
                        "Cambios guardados para la operación temporal {TemporaryClientOperationId}",
                        current.TemporaryClientOperationId);

                    var trustEvent = new CreateTrustRequestedIntegrationEvent(
                        op.AffiliateId,
                        op.ClientOperationId,
                        DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                        op.ObjectiveId,
                        op.PortfolioId,
                        op.Amount,
                        0m,
                        op.Amount,
                        0m,
                        aux.TaxConditionId,
                        aux.ContingentWithholding,
                        0m,
                        op.Amount,
                        true);
                    logger.LogInformation(
                        "Publicando evento de creación de fideicomiso para la operación {ClientOperationId}",
                        op.ClientOperationId);
                    await eventBus.PublishAsync(trustEvent, cancellationToken);

                    tempOpIds.Add(current.TemporaryClientOperationId);
                    tempAuxIds.Add(aux.TemporaryAuxiliaryInformationId);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await tx.RollbackAsync(cancellationToken);
                    logger.LogWarning(
                        ex,
                        "Concurrencia detectada procesando la operación temporal {TemporaryClientOperationId}",
                        temp.TemporaryClientOperationId);
                }
            }

            if (tempOpIds.Count > 0)
            {
                await cleanupService.ScheduleCleanupAsync(tempOpIds, tempAuxIds, cancellationToken);
                logger.LogInformation("Limpieza programada de operaciones temporales: {TemporaryClientOperationIds} y auxiliares temporales: {TemporaryAuxiliaryInformationIds}",
                    string.Join(", ", tempOpIds), string.Join(", ", tempAuxIds));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError("Error procesando contribuciones pendientes para el portafolio {PortfolioId}: {Message}", request.PortfolioId, ex.Message);
            return Result.Failure(new Error("ErrorProcesandoContribuciones", "Ocurrió un error al procesar las contribuciones pendientes.", ErrorType.Failure));

        }
   
    }
}
