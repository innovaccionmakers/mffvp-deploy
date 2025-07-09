namespace Operations.Application.Abstractions.Services.Cleanup;

public interface ITempClientOperationsCleanupService
{
    Task ScheduleCleanupAsync(
        IReadOnlyCollection<long> tempOperationIds,
        IReadOnlyCollection<long> tempAuxiliaryIds,
        CancellationToken cancellationToken = default);
}