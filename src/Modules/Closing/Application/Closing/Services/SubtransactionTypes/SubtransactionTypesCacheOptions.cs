namespace Closing.Application.Closing.Services.SubtransactionTypes;

public sealed class SubtransactionTypesCacheOptions
{
    public string Key { get; init; } = "operations:subtransactionTypes";
    public TimeSpan Ttl { get; init; } = TimeSpan.FromHours(24);
}

