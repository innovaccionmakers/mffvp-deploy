namespace Common.SharedKernel.Infrastructure.Caching.OperationTypes;

public sealed class OperationTypesCacheOptions
{
    public string Key { get; init; } = "operations:operationTypes";
    public TimeSpan Ttl { get; init; } = TimeSpan.FromHours(24);
}
