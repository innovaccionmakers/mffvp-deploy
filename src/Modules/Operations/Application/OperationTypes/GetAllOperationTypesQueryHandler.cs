using System.Text.Json;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using Microsoft.Extensions.Caching.Distributed;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;

namespace Operations.Application.OperationTypes;

public class GetAllOperationTypesQueryHandler(
    IOperationTypeRepository repository,
    IDistributedCache cache)
    : IQueryHandler<GetAllOperationTypesQuery, IReadOnlyCollection<OperationTypeResponse>>
{
    private const string CacheKey = "operations:operationtypes:all";
    private static readonly JsonSerializerOptions _serializerOptions = new();

    private record CacheModel(
        long Id,
        string Name,
        string? Category,
        IncomeEgressNature Nature,
        Status Status,
        string External,
        string HomologatedCode);

    public async Task<Result<IReadOnlyCollection<OperationTypeResponse>>> Handle(
        GetAllOperationTypesQuery request,
        CancellationToken cancellationToken)
    {
        var json = await cache.GetStringAsync(CacheKey, cancellationToken);
        if (json is not null)
        {
            var cached = JsonSerializer.Deserialize<IReadOnlyCollection<CacheModel>>(json, _serializerOptions);
            if (cached is not null)
            {
                var listFromCache = cached
                    .Select(c => new OperationTypeResponse(
                        c.Id,
                        c.Name,
                        c.Category,
                        c.Nature,
                        c.Status,
                        c.External,
                        c.HomologatedCode))
                    .ToList();
                return Result.Success((IReadOnlyCollection<OperationTypeResponse>)listFromCache);
            }
        }

        var list = await repository.GetAllAsync(cancellationToken);
        var categoryMap = list.ToDictionary(x => x.OperationTypeId, x => x.Name);

        var response = list.Select(s =>
        {
            string? categoryName = null;
            if (s.CategoryId.HasValue && categoryMap.TryGetValue((long)s.CategoryId.Value, out var cat))
            {
                categoryName = cat;
            }

            return new OperationTypeResponse(
                s.OperationTypeId,
                s.Name,
                categoryName,
                s.Nature,
                s.Status,
                s.External,
                s.HomologatedCode);
        }).ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        var cacheData = response.Select(s => new CacheModel(
            s.OperationTypeId,
            s.Name,
            s.Category,
            s.Nature,
            s.Status,
            s.External,
            s.HomologatedCode)).ToList();

        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(cacheData, _serializerOptions), options, cancellationToken);

        return Result.Success((IReadOnlyCollection<OperationTypeResponse>)response);
    }
}