using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Common.SharedKernel.Application.Caching.OperationTypes;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.Caching.OperationTypes;
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

    public async Task<Result<IReadOnlyCollection<OperationTypeInfo>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cachedJson = await cache.GetStringAsync(_cacheKey, cancellationToken);
            if (cachedJson is not null)
            {
                var cachedOperationTypesList = JsonSerializer.Deserialize<List<CachedOperationTypes>>(cachedJson);

                if (cachedOperationTypesList is not null)
                {
                    var listFromCache = cachedOperationTypesList
                        .Select(c => new OperationTypeInfo(
                            OperationTypeId: c.Id,
                            Name: c.Name,
                            Category: c.Category,
                            Nature: c.Nature,
                            Status: c.Status,
                            External: c.External,
                            HomologatedCode: c.HomologatedCode,
                            AdditionalAttributes: c.AdditionalAttributes))
                        .ToList();

                    return Result.Success((IReadOnlyCollection<OperationTypeInfo>)listFromCache);
                }
            }

            var resultFromLocator = await locator.GetAllOperationTypesAsync(cancellationToken);
            if (resultFromLocator.IsFailure)
                return resultFromLocator;

            var operationTypesToCache = resultFromLocator.Value
                .Select(c => new CachedOperationTypes(
                    Id: c.OperationTypeId,
                    Name: c.Name,
                    Category: c.Category,
                    Nature: c.Nature,
                    Status: c.Status,
                    External: c.External,
                    HomologatedCode: c.HomologatedCode,
                    AdditionalAttributes: c.AdditionalAttributes))
                .ToList();
            var serialized = JsonSerializer.Serialize(operationTypesToCache);
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
