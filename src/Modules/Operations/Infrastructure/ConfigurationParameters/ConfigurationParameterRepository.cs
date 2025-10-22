using Operations.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Operations.Infrastructure.Database;
using Operations.Application.Abstractions;
using Common.SharedKernel.Domain;

namespace Operations.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository :
    IConfigurationParameterRepository,
    IConfigurationParameterLookupRepository<OperationsModuleMarker>
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
                     && p.Type == scope,
                cancellationToken);
    }

    public async Task<ConfigurationParameter?> GetByIdAsync(
        long configurationParameterId,
        CancellationToken cancellationToken = default)
    {
        if (configurationParameterId is < int.MinValue or > int.MaxValue)
        {
            return null;
        }

        var id = (int)configurationParameterId;

        return await context.ConfigurationParameters
            .AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.ConfigurationParameterId == id,
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

    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetActiveConfigurationParametersByTypeAsync(
        ConfigurationParameterType type,
        CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .Where(cp => cp.Type == type.ToString() && cp.Status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetOriginModeByOriginIdAsync(int originId, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .Join(
                context.OriginModes,
                    cp => cp.ConfigurationParameterId,
                    om => om.ModalityOriginId,
                    (cp, om) => new { cp, om }
                )
            .Where(x => x.om.OriginId == originId)
            .Where(x => x.cp.Type == ConfigurationParameterType.ModalidadOrigen.ToString() && x.cp.Status)
            .Select(x => x.cp)
            .ToListAsync(cancellationToken);
    }
}