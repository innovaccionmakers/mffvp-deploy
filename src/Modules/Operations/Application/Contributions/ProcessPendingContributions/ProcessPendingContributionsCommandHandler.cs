using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Integrations.Contributions.ProcessPendingContributions;
using Trusts.IntegrationEvents.CreateTrustRequested;

namespace Operations.Application.Contributions.ProcessPendingContributions;

internal sealed class ProcessPendingContributionsCommandHandler(
    IPendingTransactionsReaderRepository optimizedReader,     
    ITemporaryClientOperationRepository tempOpRepo,       
    ITempClientOperationsCleanupService cleanupService, 
    ITransactionControl transactionControl,
    IOperationCompleted operationCompleted,
    IEventBus eventBus,
    IUnitOfWork unitOfWork,
    ILogger<ProcessPendingContributionsCommandHandler> logger)
    : ICommandHandler<ProcessPendingContributionsCommand>
{
    private const int BatchSize = 500; 

    public async Task<Result> Handle(ProcessPendingContributionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var allProcessedTempIds = new List<long>();
            var allProcessedAuxIds = new List<long>();
            var processDateUtc = DateTimeConverter.ToUtcDateTime(request.ProcessDate);

            while (true)
            {
                // 1) Tomar un batch de temporales + auxiliar, con lock y sin tracking
                var batch = await optimizedReader
                    .TakePendingBatchWithAuxAsync(request.PortfolioId, BatchSize, cancellationToken);

                if (batch.Count == 0) break;

                await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var createdOps = new List<ClientOperation>(batch.Count);
                    var createdAux = new List<AuxiliaryInformation>(batch.Count);

                    // 2) Mapear y preparar entidades finales en memoria
                    foreach (var r in batch)
                    {
                        var op = ClientOperation.Create(
                            r.RegistrationDate,
                            r.AffiliateId,
                            r.ObjectiveId,
                            r.PortfolioId,
                            r.Amount,
                            processDateUtc,
                            r.OperationTypeId,
                            DateTime.UtcNow,
                            r.Status,
                            r.CauseId,
                            r.TrustId,
                            r.LinkedClientOperationId,
                            r.Units).Value;

                        var info = AuxiliaryInformation.Create(
                            op.ClientOperationId,
                            r.OriginId,
                            r.CollectionMethodId,
                            r.PaymentMethodId,
                            r.CollectionAccount,
                            r.PaymentMethodDetail,
                            r.CertificationStatusId,
                            r.TaxConditionId,
                            r.ContingentWithholding,
                            r.VerifiableMedium,
                            r.CollectionBankId,
                            r.DepositDate,
                            r.SalesUser,
                            r.OriginModalityId,
                            r.CityId,
                            r.ChannelId,
                            r.UserId).Value;

                        createdOps.Add(op);
                        createdAux.Add(info);
                    }

                    // 3) Ejecutar inserciones dentro de la misma transacción
                    foreach (var (op, info) in createdOps.Zip(createdAux))
                        await transactionControl.ExecuteAsync(op, info, cancellationToken);

                    // 4) Un solo SaveChanges para el batch
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    // 5) Marcar temporales procesadas en bulk
                    var idsToMark = batch.Select(x => x.TemporaryClientOperationId).ToArray();
                    var affected = await tempOpRepo.MarkProcessedBulkIfPendingAsync(idsToMark, cancellationToken);

                    if (affected < idsToMark.Length)
                    {
                        logger.LogWarning("Batch marcadas {Affected}/{Total}. Algunas ya estaban procesadas por otra instancia.",
                            affected, idsToMark.Length);
                    }

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await tx.CommitAsync(cancellationToken);

                    // 6) Publicaciones posteriores al commit 
                    foreach (var (op, r) in createdOps.Zip(batch))
                    {
                        //Réplica de operación a Closing
                        await operationCompleted.ExecuteAsync(op, cancellationToken);

                        var trustEvent = new CreateTrustRequestedIntegrationEvent(
                            op.AffiliateId,
                            op.ClientOperationId,
                            DateTime.UtcNow,
                            op.ObjectiveId,
                            op.PortfolioId,
                            op.Amount,
                            0m, op.Amount, 0m,
                            r.TaxConditionId, r.ContingentWithholding,
                            0m, op.Amount, LifecycleStatus.Active);
                        //Creación de fideicomiso 
                        await eventBus.PublishAsync(trustEvent, cancellationToken);
                    }

                    allProcessedTempIds.AddRange(idsToMark);
                    allProcessedAuxIds.AddRange(batch.Select(x => x.TemporaryAuxiliaryInformationId));
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            // 7) Borrado diferido
            if (allProcessedTempIds.Count > 0)
                await cleanupService.ScheduleCleanupAsync(allProcessedTempIds, allProcessedAuxIds, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando contribuciones pendientes para el portafolio {PortfolioId}", request.PortfolioId);
            return Result.Failure(new Error(
                "ErrorProcesandoContribuciones",
                "Ocurrió un error al procesar las contribuciones pendientes.",
                ErrorType.Failure));
        }
    }
}
