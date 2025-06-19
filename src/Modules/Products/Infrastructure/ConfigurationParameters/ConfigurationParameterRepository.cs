using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Products.Application.Abstractions;
using Products.Domain.ConfigurationParameters;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository :
    IConfigurationParameterRepository,
    IConfigurationParameterLookupRepository<ProductsModuleMarker>
{
    private readonly ProductsDbContext _context;

    public ConfigurationParameterRepository(ProductsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationParameters
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationParameter?> GetAsync(int configurationParameterId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationParameters
            .SingleOrDefaultAsync(x => x.ConfigurationParameterId == configurationParameterId, cancellationToken);
    }

    public void Insert(ConfigurationParameter parameter)
    {
        _context.ConfigurationParameters.Add(parameter);
    }

    public void Update(ConfigurationParameter parameter)
    {
        _context.ConfigurationParameters.Update(parameter);
    }

    public void Delete(ConfigurationParameter parameter)
    {
        _context.ConfigurationParameters.Remove(parameter);
    }

    public async Task<ConfigurationParameter?> GetByUuidAsync(
        Guid uuid,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ConfigurationParameters
            .SingleOrDefaultAsync(
                p => p.Uuid == uuid,
                cancellationToken
            );
    }

    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken ct = default)
    {
        var idArray = ids.ToArray();
        if (idArray.Length == 0)
            return Array.Empty<ConfigurationParameter>();

        return await _context.ConfigurationParameters
            .Where(p => idArray.Contains(p.ConfigurationParameterId))
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationParameters
            .AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.HomologationCode == homologationCode
                     && p.Name == scope,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetActiveConfigurationParametersByTypeAsync(
       ConfigurationParameterType type,
       CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationParameters
            .Where(cp => cp.Type == type.ToString() && cp.Status)
            .ToListAsync(cancellationToken);
    }
}