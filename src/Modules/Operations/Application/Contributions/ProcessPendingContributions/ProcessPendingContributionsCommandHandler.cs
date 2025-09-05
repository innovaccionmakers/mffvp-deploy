using Azure;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Contributions.Services.OperationCompleted;
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
     IOperationCompleted operationCompleted,
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
            var processedTempIds = new List<long>();
            var processedAuxIds = new List<long>();
            var processDate = DateTimeConverter.ToUtcDateTime(request.ProcessDate);

            while (true)
            {
                // 1) Traer siguiente id pendiente (sin tracking)
                var nextId = await tempOpRepo.GetNextPendingIdAsync(request.PortfolioId, cancellationToken);
                if (nextId is null) break;

                await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // 2) Cargar la fila (con tracking)
                    var current = await tempOpRepo.GetAsync(nextId.Value, cancellationToken);
                    if (current is null || current.Processed)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        continue;
                    }

                    var aux = await tempAuxRepo.GetAsync(current.TemporaryClientOperationId, cancellationToken);
                    if (aux is null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        continue;
                    }

                    // 3) Crear operación final
                    var op = ClientOperation.Create(
                        current.RegistrationDate,
                        current.AffiliateId,
                        current.ObjectiveId,
                        current.PortfolioId,
                        current.Amount,
                        processDate,
                        current.OperationTypeId,
                        DateTime.UtcNow).Value;


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

                    // 4) Guardar inserción 
                    try
                    {
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    catch (DbUpdateException ex) when (IsUniqueViolation23505(ex))
                    {
                        logger.LogInformation("Transaccion temporal {TemporaryClientOperationId}, valor {Amount} ya insertada: {Message}", current.TemporaryClientOperationId, current.Amount, ex.Message);
                    }

                    // 5) Marcar procesado
                    var affected = await tempOpRepo.MarkProcessedIfPendingAsync(
                        current.TemporaryClientOperationId,
                        cancellationToken);

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    if (affected == 0)
                    {
                        logger.LogInformation("Transaccion temporal {TemporaryClientOperationId}, valor {Amount} ya marcada por otra instancia", current.TemporaryClientOperationId, current.Amount);
                        await transaction.RollbackAsync(cancellationToken);
                        continue;
                    }

                    await transaction.CommitAsync(cancellationToken);

                    // 6) Publicar eventos después del commit

                    await operationCompleted.ExecuteAsync(op, cancellationToken);

                    var trustEvent = new CreateTrustRequestedIntegrationEvent(
                        op.AffiliateId,
                        op.ClientOperationId,
                        DateTime.UtcNow,
                        op.ObjectiveId,
                        op.PortfolioId,
                        op.Amount,
                        0m, op.Amount, 0m,
                        aux.TaxConditionId, aux.ContingentWithholding,
                        0m, op.Amount, true);

                    await eventBus.PublishAsync(trustEvent, cancellationToken);

                    processedTempIds.Add(current.TemporaryClientOperationId);
                    processedAuxIds.Add(aux.TemporaryAuxiliaryInformationId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    logger.LogInformation("DbUpdateConcurrencyException para transaccion temporal {TemporaryClientOperationId}, otra sesión tocó la fila", nextId);
                    await transaction.RollbackAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            if (processedTempIds.Count > 0)
                await cleanupService.ScheduleCleanupAsync(processedTempIds, processedAuxIds, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError("Error procesando contribuciones pendientes para el portafolio {PortfolioId}: {Message}", request.PortfolioId, ex.Message);
            return Result.Failure(new Error("ErrorProcesandoContribuciones", "Ocurrió un error al procesar las contribuciones pendientes.", ErrorType.Failure));

        }

        static bool IsUniqueViolation23505(DbUpdateException ex)
      => ex.InnerException?.Message.Contains("23505") == true
         || ex.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase);

    }
}
