using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Closing.Application.Closing.Services.SubtransactionTypes;

public class CachedSubtransactionTypesService(
    ISubtransactionTypesLocator locator,
    IDistributedCache cache,
    ILogger<CachedSubtransactionTypesService> logger,
    IOptions<SubtransactionTypesCacheOptions>? cacheOptions = null)
    : ISubtransactionTypesService
{
    private readonly string _cacheKey = cacheOptions?.Value.Key ?? new SubtransactionTypesCacheOptions().Key;
    private readonly TimeSpan _ttl = cacheOptions?.Value.Ttl ?? new SubtransactionTypesCacheOptions().Ttl;

    public async Task<Result<IReadOnlyCollection<SubtransactionTypesRemoteResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cached = await cache.GetStringAsync(_cacheKey, cancellationToken);
            if (cached is not null)
            {
                var result = JsonSerializer.Deserialize<List<SubtransactionTypesRemoteResponse>>(cached);
                return Result.Success<IReadOnlyCollection<SubtransactionTypesRemoteResponse>>(result!);
            }

            var resultFromLocator = await locator.GetAllSubtransactionTypesAsync(cancellationToken);
            if (resultFromLocator.IsFailure)
                return resultFromLocator;

            var serialized = JsonSerializer.Serialize(resultFromLocator.Value);
            await cache.SetStringAsync(_cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _ttl
            }, cancellationToken);

            return resultFromLocator;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CachedSubtransactionTypesService] Error accediendo a cache o RPC");
            return await locator.GetAllSubtransactionTypesAsync(cancellationToken);
        }
    }
}

