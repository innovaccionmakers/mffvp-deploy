using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Closing.Application.Closing.Services.OperationTypes;

public class CachedOperationTypesService(
    IOperationTypesLocator locator,
    IDistributedCache cache,
    ILogger<CachedOperationTypesService> logger,
    IOptions<OperationTypesCacheOptions>? cacheOptions = null)
    : IOperationTypesService
{
    private readonly string _cacheKey = cacheOptions?.Value.Key ?? new OperationTypesCacheOptions().Key;
    private readonly TimeSpan _ttl = cacheOptions?.Value.Ttl ?? new OperationTypesCacheOptions().Ttl;

    public async Task<Result<IReadOnlyCollection<OperationTypesRemoteResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cached = await cache.GetStringAsync(_cacheKey, cancellationToken);
            if (cached is not null)
            {
                var result = JsonSerializer.Deserialize<List<OperationTypesRemoteResponse>>(cached);
                return Result.Success<IReadOnlyCollection<OperationTypesRemoteResponse>>(result!);
            }

            var resultFromLocator = await locator.GetAllOperationTypesAsync(cancellationToken);
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
            logger.LogError(ex, "[CachedOperationTypesService] Error accediendo a cache");
            return await locator.GetAllOperationTypesAsync(cancellationToken);
        }
    }
}
