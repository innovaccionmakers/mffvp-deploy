using Closing.Domain.ConfigurationParameters;
using Closing.Infrastructure.Database;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository : IConfigurationParameterRepository
{
    private readonly ClosingDbContext context;
    
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
}