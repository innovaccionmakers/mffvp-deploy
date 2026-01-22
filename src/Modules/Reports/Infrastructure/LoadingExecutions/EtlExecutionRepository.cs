using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Infrastructure.Database;
using System.Text.Json;

namespace Reports.Infrastructure.LoadingExecutions;

public sealed class EtlExecutionRepository : IEtlExecutionRepository
{
    private readonly ReportsWriteDbContext dbContext;

    private readonly IDbContextFactory<ReportsWriteDbContext> dbContextFactory;

    public EtlExecutionRepository(ReportsWriteDbContext dbContext, IDbContextFactory<ReportsWriteDbContext> dbContextFactory)
    {
        this.dbContext = dbContext;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Result<long>> InsertRunningAsync(
        string executionName,
        JsonDocument? parametersJson,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken)
    {
        var entity = EtlExecution.Start(
            name: executionName,
            parameters: parametersJson,
            startedAtUtc: startedAtUtc);

        dbContext.Set<EtlExecution>().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }

    public async Task<Result> FinalizeCompletedAsync(
        long executionId,
        DateTimeOffset finishedAtUtc,
        long durationMilliseconds,
        JsonDocument parametersFinalJson,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.Set<EtlExecution>()
            .TagWith("[EtlExecutionRepository]_[FinalizeCompletedAsync]_Update")
            .Where(x => x.Id == executionId && x.Status == EtlExecutionStatus.Running)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, EtlExecutionStatus.Completed)
                .SetProperty(x => x.FinishedAtUtc, finishedAtUtc)
                .SetProperty(x => x.DurationMilliseconds, durationMilliseconds)
                .SetProperty(x => x.Parameters, parametersFinalJson)
                .SetProperty(x => x.Error, (JsonDocument?)null),
                cancellationToken);

        if (affectedRows == 1)
            return Result.Success();

        return await ResolveFinalizeConflictAsync(executionId, cancellationToken);
    }

    public async Task<Result> FinalizeFailedAsync(
        long executionId,
        DateTimeOffset finishedAtUtc,
        long durationMilliseconds,
        JsonDocument parametersFinalJson,
        JsonDocument errorJson,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.Set<EtlExecution>()
            .TagWith("[EtlExecutionRepository]_[FinalizeFailedAsync]_Update")
            .Where(x => x.Id == executionId && x.Status == EtlExecutionStatus.Running)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, EtlExecutionStatus.Failed)
                .SetProperty(x => x.FinishedAtUtc, finishedAtUtc)
                .SetProperty(x => x.DurationMilliseconds, durationMilliseconds)
                .SetProperty(x => x.Parameters, parametersFinalJson)
                .SetProperty(x => x.Error, errorJson),
                cancellationToken);

        if (affectedRows == 1)
            return Result.Success();

        return await ResolveFinalizeConflictAsync(executionId, cancellationToken);
    }

    public async Task<long?> GetLastSuccessfulExecutionTimestampAsync(
        string executionName,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var lastCompleted = await dbContext.Set<EtlExecution>()
            .AsNoTracking()
            .TagWith("[EtlExecutionRepository]_[GetLastSuccessfulExecutionTimestampAsync]")
            .Where(x => x.Name == executionName && x.Status == EtlExecutionStatus.Completed)
            .OrderByDescending(x => x.FinishedAtUtc)
            .Select(x => x.FinishedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastCompleted == null)
            return null;

        // Convertir DateTimeOffset a milisegundos UNIX EPOCH
        return lastCompleted.Value.ToUnixTimeMilliseconds();
    }

    private async Task<Result> ResolveFinalizeConflictAsync(long executionId, CancellationToken cancellationToken)
    {
        var current = await dbContext.Set<EtlExecution>()
            .AsNoTracking()
            .TagWith("[EtlExecutionRepository]_[ResolveFinalizeConflictAsync]_Check")
            .Where(x => x.Id == executionId)
            .Select(x => new { x.Id, x.Status })
            .SingleOrDefaultAsync(cancellationToken);

        if (current is null)
            return Result.Failure(
              new Error("ETL404", $"Execución no encontrada. executionId={executionId}", ErrorType.Failure));

        return Result.Failure(
     new Error("ETL409", $"Execución ya finalizó (status='{current.Status}'). executionId={executionId}", ErrorType.Conflict));
    }
}