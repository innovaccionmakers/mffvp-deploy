using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;

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

            var operations = await tempOpRepo.GetByIdsAsync(operationIds, stoppingToken);
            if (operations.Count == 0)
            {
                continue;
            }

            var auxiliaries = await tempAuxRepo.GetByIdsAsync(auxiliaryIds, stoppingToken);

            await using var transaction = await unitOfWork.BeginTransactionAsync(stoppingToken);
            tempOpRepo.DeleteRange(operations);
            tempAuxRepo.DeleteRange(auxiliaries);
            await unitOfWork.SaveChangesAsync(stoppingToken);
            await transaction.CommitAsync(stoppingToken);
        }
    }
}