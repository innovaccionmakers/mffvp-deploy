using Microsoft.EntityFrameworkCore;
using Operations.Domain.ConfigurationParameters;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository : IConfigurationParameterRepository
{
    private readonly OperationsDbContext context;

    public ConfigurationParameterRepository(OperationsDbContext context)
    {
        this.context = context;
    }

    public async Task<ConfigurationParameter?> GetByUuidAsync(
        Guid uuid,
        CancellationToken cancellationToken = default
    )
    {
        return await context.ConfigurationParameters
            .SingleOrDefaultAsync(
                p => p.Uuid == uuid,
                cancellationToken
            );
    }

    public async Task<IReadOnlyDictionary<(string Code, string Scope), ConfigurationParameter>>
        GetByCodesAndTypesAsync(IEnumerable<(string Code, string Scope)> pairs, CancellationToken ct)
    {
        var list = pairs.ToList();
        var codes = list.Select(x => x.Code).Distinct().ToList();
        var scopes = list.Select(x => x.Scope).Distinct().ToList();

        var items = await context.ConfigurationParameters
            .AsNoTracking()
            .Where(p => codes.Contains(p.HomologationCode)
                        && scopes.Contains(p.Type))
            .ToListAsync(ct);

        return items.ToDictionary(p => (p.HomologationCode, p.Type));
    }

    public async Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.HomologationCode == homologationCode
                     && p.Name == scope,
                cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, ConfigurationParameter>>
        GetByUuidsAsync(IEnumerable<Guid> uuids, CancellationToken cancellationToken)
    {
        var items = await context.ConfigurationParameters
            .Where(p => uuids.Contains(p.Uuid))
            .ToListAsync(cancellationToken);
        return items.ToDictionary(p => p.Uuid);
    }
}