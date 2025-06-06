using Microsoft.EntityFrameworkCore;
using Associate.Domain.ConfigurationParameters;
using Associate.Infrastructure.Database;

namespace Associate.Infrastructure;

internal sealed class ConfigurationParameterRepository(AssociateDbContext context) : IConfigurationParameterRepository
{
    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters.ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationParameter?> GetAsync(int configurationparameterId, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .SingleOrDefaultAsync(x => x.ConfigurationParameterId == configurationparameterId, cancellationToken);
    }

    public void Insert(ConfigurationParameter configurationparameter)
    {
        context.ConfigurationParameters.Add(configurationparameter);
    }

    public void Update(ConfigurationParameter configurationparameter)
    {
        context.ConfigurationParameters.Update(configurationparameter);
    }

    public void Delete(ConfigurationParameter configurationparameter)
    {
        context.ConfigurationParameters.Remove(configurationparameter);
    }

    public async Task<ConfigurationParameter?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .SingleOrDefaultAsync(p => p.Uuid == uuid, cancellationToken);
    }

    public async Task<bool> GetByCodeAndScopeAsync(string homologationCode, string scope, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters.AnyAsync(p => p.HomologationCode == homologationCode && p.Type == scope, cancellationToken);
    }
}