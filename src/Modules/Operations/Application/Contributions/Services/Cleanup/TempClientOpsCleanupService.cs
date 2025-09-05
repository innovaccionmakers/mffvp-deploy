using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;
using System.Diagnostics;
using System.Threading.Channels;

namespace Operations.Application.Contributions.Services.Cleanup;

public sealed class TempClientOpsCleanupService : BackgroundService, ITempClientOperationsCleanupService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly Channel<(IReadOnlyCollection<long> operationIds, IReadOnlyCollection<long> auxiliaryIds)> cleanupChannel;

    public TempClientOpsCleanupService(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
        cleanupChannel = Channel.CreateUnbounded<(IReadOnlyCollection<long>, IReadOnlyCollection<long>)>();
    }

    public Task ScheduleCleanupAsync(
        IReadOnlyCollection<long> tempOperationIds,
        IReadOnlyCollection<long> tempAuxiliaryIds,
        CancellationToken cancellationToken = default)
    {
        cleanupChannel.Writer.TryWrite((tempOperationIds, tempAuxiliaryIds));
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (operationIds, auxiliaryIds) in cleanupChannel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var tempOpRepo = scope.ServiceProvider.GetRequiredService<ITemporaryClientOperationRepository>();
            var tempAuxRepo = scope.ServiceProvider.GetRequiredService<ITemporaryAuxiliaryInformationRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await using var transaction = await unitOfWork.BeginTransactionAsync(stoppingToken);

            await tempAuxRepo.DeleteByIdsAsync(auxiliaryIds, stoppingToken);
            await tempOpRepo.DeleteByIdsAsync(operationIds, stoppingToken);

            await transaction.CommitAsync(stoppingToken);
        }
    }

    //public Task ScheduleCleanupAsync(
    //   IReadOnlyCollection<long> tempOperationIds,
    //   IReadOnlyCollection<long> tempAuxiliaryIds,
    //   CancellationToken cancellationToken = default)
    //{
    //    var cleanupId = Guid.NewGuid();

    //    logger.LogInformation("[CLEANUP-SCHEDULE] CleanupId: {CleanupId} - Programando limpieza de {TempOpsCount} operaciones temporales y {TempAuxCount} auxiliares temporales - TempOperationIds: [{TempOperationIds}], TempAuxiliaryIds: [{TempAuxiliaryIds}]",
    //        cleanupId, tempOperationIds.Count, tempAuxiliaryIds.Count, string.Join(", ", tempOperationIds), string.Join(", ", tempAuxiliaryIds));

    //    _ = Task.Run(async () =>
    //    {
    //        var taskStopwatch = Stopwatch.StartNew();

    //        logger.LogInformation("[CLEANUP-TASK-START] CleanupId: {CleanupId} - Iniciando tarea de limpieza en background", cleanupId);

    //        try
    //        {
    //            logger.LogInformation("[CLEANUP-CREATE-SCOPE] CleanupId: {CleanupId} - Creando scope para servicios de limpieza", cleanupId);

    //            using var scope = scopeFactory.CreateScope();

    //            logger.LogInformation("[CLEANUP-GET-SERVICES] CleanupId: {CleanupId} - Obteniendo servicios requeridos", cleanupId);

    //            var tempOpRepo = scope.ServiceProvider.GetRequiredService<ITemporaryClientOperationRepository>();
    //            var tempAuxRepo = scope.ServiceProvider.GetRequiredService<ITemporaryAuxiliaryInformationRepository>();
    //            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    //            logger.LogInformation("[CLEANUP-GET-TEMP-OPS] CleanupId: {CleanupId} - Obteniendo operaciones temporales para eliminación - IDs: [{TempOperationIds}]",
    //                cleanupId, string.Join(", ", tempOperationIds));

    //            var ops = await tempOpRepo.GetByIdsAsync(tempOperationIds, cancellationToken);

    //            logger.LogInformation("[CLEANUP-TEMP-OPS-FOUND] CleanupId: {CleanupId} - Se encontraron {OpsFoundCount} de {OpsRequestedCount} operaciones temporales",
    //                cleanupId, ops.Count, tempOperationIds.Count);

    //            if (ops.Count == 0)
    //            {
    //                logger.LogWarning("[CLEANUP-NO-TEMP-OPS] CleanupId: {CleanupId} - No se encontraron operaciones temporales para eliminar. Cancelando limpieza.", cleanupId);
    //                return;
    //            }

    //            logger.LogInformation("[CLEANUP-GET-TEMP-AUX] CleanupId: {CleanupId} - Obteniendo información auxiliar temporal para eliminación - IDs: [{TempAuxiliaryIds}]",
    //                cleanupId, string.Join(", ", tempAuxiliaryIds));

    //            var auxList = await tempAuxRepo.GetByIdsAsync(tempAuxiliaryIds, cancellationToken);

    //            logger.LogInformation("[CLEANUP-TEMP-AUX-FOUND] CleanupId: {CleanupId} - Se encontraron {AuxFoundCount} de {AuxRequestedCount} información auxiliar temporal",
    //                cleanupId, auxList.Count, tempAuxiliaryIds.Count);

    //            logger.LogInformation("[CLEANUP-BEGIN-TX] CleanupId: {CleanupId} - Iniciando transacción de limpieza", cleanupId);

    //            await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

    //            logger.LogInformation("[CLEANUP-TX-STARTED] CleanupId: {CleanupId} - Transacción de limpieza iniciada exitosamente", cleanupId);

    //            logger.LogInformation("[CLEANUP-DELETE-OPS] CleanupId: {CleanupId} - Eliminando {OpsCount} operaciones temporales - IDs: [{OperationIds}]",
    //                cleanupId, ops.Count, string.Join(", ", ops.Select(o => o.TemporaryClientOperationId)));

    //            tempOpRepo.DeleteRange(ops);

    //            logger.LogInformation("[CLEANUP-DELETE-AUX] CleanupId: {CleanupId} - Eliminando {AuxCount} información auxiliar temporal - IDs: [{AuxiliaryIds}]",
    //                cleanupId, auxList.Count, string.Join(", ", auxList.Select(a => a.TemporaryAuxiliaryInformationId)));

    //            tempAuxRepo.DeleteRange(auxList);

    //            logger.LogInformation("[CLEANUP-SAVE-CHANGES] CleanupId: {CleanupId} - Guardando cambios de eliminación", cleanupId);

    //            await unitOfWork.SaveChangesAsync(cancellationToken);

    //            logger.LogInformation("[CLEANUP-CHANGES-SAVED] CleanupId: {CleanupId} - Cambios de eliminación guardados exitosamente", cleanupId);

    //            logger.LogInformation("[CLEANUP-COMMIT] CleanupId: {CleanupId} - Confirmando transacción de limpieza", cleanupId);

    //            await tx.CommitAsync(cancellationToken);

    //            taskStopwatch.Stop();
    //            logger.LogInformation("[CLEANUP-COMMIT-OK] CleanupId: {CleanupId} - Transacción de limpieza confirmada exitosamente en {ElapsedMs}ms - Eliminadas {OpsCount} operaciones temporales y {AuxCount} auxiliares temporales",
    //                cleanupId, taskStopwatch.ElapsedMilliseconds, ops.Count, auxList.Count);
    //        }
    //        catch (Exception ex)
    //        {
    //            taskStopwatch.Stop();
    //            logger.LogError(ex, "[CLEANUP-ERROR] CleanupId: {CleanupId} - Error durante la limpieza después de {ElapsedMs}ms - TempOperationIds: [{TempOperationIds}], TempAuxiliaryIds: [{TempAuxiliaryIds}]: {Message} - StackTrace: {StackTrace}",
    //                cleanupId, taskStopwatch.ElapsedMilliseconds, string.Join(", ", tempOperationIds), string.Join(", ", tempAuxiliaryIds), ex.Message, ex.StackTrace);
    //        }
    //    }, cancellationToken);

    //    logger.LogInformation("[CLEANUP-SCHEDULED] CleanupId: {CleanupId} - Tarea de limpieza programada exitosamente", cleanupId);

    //    return Task.CompletedTask;
    //}
}
