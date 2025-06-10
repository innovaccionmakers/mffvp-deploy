using Trusts.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Trusts.Infrastructure.Database;

namespace Trusts.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository : IConfigurationParameterRepository
{
    private readonly TrustsDbContext _context;

    public ConfigurationParameterRepository(TrustsDbContext context)
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
}