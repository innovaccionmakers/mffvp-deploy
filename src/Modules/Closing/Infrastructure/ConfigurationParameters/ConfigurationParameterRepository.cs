using Closing.Domain.ConfigurationParameters;
using Closing.Infrastructure.Database;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Closing.Application.Abstractions;

namespace Closing.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository :
    IConfigurationParameterRepository,
    IConfigurationParameterLookupRepository<ClosingModuleMarker>
{
    private readonly ClosingDbContext context;
    
    public ConfigurationParameterRepository(ClosingDbContext context)
    {
        this.context = context;
    }
    
    public async Task<ConfigurationParameter?> GetByUuidAsync(
        Guid uuid,
        CancellationToken cancellationToken = default
    )
    {
        return await context.ConfigurationParameters
            .TagWith("ConfigurationParameterRepository_GetByUuidAsync")
            .SingleOrDefaultAsync(
                p => p.Uuid == uuid,
                cancellationToken
            );
    }
}