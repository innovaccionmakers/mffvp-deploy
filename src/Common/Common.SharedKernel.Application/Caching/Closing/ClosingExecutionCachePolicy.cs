
namespace Common.SharedKernel.Application.Caching.Closing;

public class ClosingExecutionCachePolicy
{
    public TimeSpan Expiration { get; init; } = TimeSpan.FromHours(24);
}

