using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Treasury.Application.Abstractions;
using Treasury.Domain.ConfigurationParameters;
using Treasury.Infrastructure.Database;

namespace Treasury.Infrastructure.ConfigurationParameters;

public class ConfigurationParameterRepository(TreasuryDbContext context) : IConfigurationParameterRepository, IConfigurationParameterLookupRepository<TreasuryModuleMarker>
{
    public async Task<IReadOnlyCollection<ConfigurationParameter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
           .ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationParameter?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .SingleOrDefaultAsync(
                p => p.Uuid == uuid,
                cancellationToken
            );
    }
}
