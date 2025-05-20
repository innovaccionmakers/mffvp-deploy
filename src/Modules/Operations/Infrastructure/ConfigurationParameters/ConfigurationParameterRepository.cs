using Microsoft.EntityFrameworkCore;
using Operations.Domain.ConfigurationParameters;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository : IConfigurationParameterRepository
{
    private readonly OperationsDbContext _context;

    public ConfigurationParameterRepository(OperationsDbContext context)
    {
        _context = context;
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