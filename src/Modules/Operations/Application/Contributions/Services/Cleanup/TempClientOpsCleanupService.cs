using Microsoft.Extensions.DependencyInjection;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;

namespace Operations.Application.Contributions.Services.Cleanup;

public sealed class TempClientOpsCleanupService(IServiceScopeFactory scopeFactory)
    : ITempClientOperationsCleanupService
{
    public Task ScheduleCleanupAsync(
        IReadOnlyCollection<long> tempOperationIds,
        IReadOnlyCollection<long> tempAuxiliaryIds,
        CancellationToken cancellationToken = default)
    {
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var tempOpRepo = scope.ServiceProvider.GetRequiredService<ITemporaryClientOperationRepository>();
            var tempAuxRepo = scope.ServiceProvider.GetRequiredService<ITemporaryAuxiliaryInformationRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var ops = await tempOpRepo.GetByIdsAsync(tempOperationIds, cancellationToken);
            if (ops.Count == 0) return;
            var auxList = await tempAuxRepo.GetByIdsAsync(tempAuxiliaryIds, cancellationToken);

            await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
            tempOpRepo.DeleteRange(ops);
            tempAuxRepo.DeleteRange(auxList);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }, cancellationToken);

        return Task.CompletedTask;
    }
}