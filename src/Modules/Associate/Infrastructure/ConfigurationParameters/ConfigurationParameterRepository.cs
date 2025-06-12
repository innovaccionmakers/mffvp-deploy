using Associate.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Associate.Infrastructure.Database;
using Associate.Application.Abstractions;

namespace Associate.Infrastructure;

internal sealed class ConfigurationParameterRepository(AssociateDbContext context) :
    IConfigurationParameterRepository,
    IConfigurationParameterLookupRepository<AssociateModuleMarker>
{
    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters.ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationParameter?> GetAsync(Guid Uuid, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .SingleOrDefaultAsync(x => x.Uuid == Uuid, cancellationToken);
    }

    public void Insert(ConfigurationParameter configurationparameter)
    {
        context.ConfigurationParameters.Add(configurationparameter);
    }

    public void Update(ConfigurationParameter configurationparameter)
    {
        context.ConfigurationParameters.Update(configurationparameter);
    }
    
    public async Task<ConfigurationParameter?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .SingleOrDefaultAsync(p => p.Uuid == uuid, cancellationToken);
    }

    public async Task<ConfigurationParameter> GetByCodeAndScopeAsync(string homologationCode, string scope, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
                    .SingleOrDefaultAsync(p => p.HomologationCode == homologationCode && p.Type == scope, cancellationToken);
    }
}