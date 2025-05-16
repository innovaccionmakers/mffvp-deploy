using Products.Domain.ConfigurationParameters;
using Products.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Products.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository : IConfigurationParameterRepository
{
    private readonly ProductsDbContext _context;

    public ConfigurationParameterRepository(ProductsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationParameters
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationParameter?> GetAsync(int configurationParameterId, CancellationToken cancellationToken = default)
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
}