using System.Text.Json;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;
using Microsoft.Extensions.Caching.Distributed;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.SubtransactionTypes;
using Operations.Integrations.SubTransactionTypes;

namespace Operations.Application.SubTransactionTypes;

public class GetAllOperationTypesQueryHandler(
    ISubtransactionTypeRepository repository,
    IConfigurationParameterRepository parameters,
    IDistributedCache cache)
    : IQueryHandler<GetAllOperationTypesQuery, IReadOnlyCollection<SubtransactionTypeResponse>>
{
    private const string CacheKey = "operations:subtransactiontypes:all";
    private static readonly JsonSerializerOptions _serializerOptions = new();

    private record CacheModel(
        long Id,
        string Name,
        string Category,
        IncomeEgressNature Nature,
        Status Status,
        string External,
        string HomologatedCode);

    public async Task<Result<IReadOnlyCollection<SubtransactionTypeResponse>>> Handle(
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
                    .Select(c => new SubtransactionTypeResponse(
                        c.Id,
                        c.Name,
                        c.Category,
                        c.Nature,
                        c.Status,
                        c.External,
                        c.HomologatedCode))
                    .ToList();
                return Result.Success((IReadOnlyCollection<SubtransactionTypeResponse>)listFromCache);
            }
        }

        var list = await repository.GetAllAsync(cancellationToken);
        var categories = await parameters.GetByUuidsAsync(list.Select(x => x.Category), cancellationToken);

        var response = list.Select(s => new SubtransactionTypeResponse(
            s.SubtransactionTypeId,
            s.Name,
            categories.TryGetValue(s.Category, out var cat) ? cat.Name : string.Empty,
            s.Nature,
            s.Status,
            s.External,
            s.HomologatedCode)).ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        var cacheData = response.Select(s => new CacheModel(
            s.SubtransactionTypeId,
            s.Name,
            s.Category,
            s.Nature,
            s.Status,
            s.External,
            s.HomologatedCode)).ToList();

        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(cacheData, _serializerOptions), options, cancellationToken);

        return Result.Success((IReadOnlyCollection<SubtransactionTypeResponse>)response);
    }
}