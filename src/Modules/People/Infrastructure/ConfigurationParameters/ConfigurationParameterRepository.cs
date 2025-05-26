using Microsoft.EntityFrameworkCore;
using People.Domain.ConfigurationParameters;
using People.Infrastructure.Database;

namespace People.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository : IConfigurationParameterRepository
{
    private readonly PeopleDbContext _context;

    public ConfigurationParameterRepository(PeopleDbContext context)
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

    public Task<ConfigurationParameter?> GetByHomologationCodeAsync(
        string homologationCode,
        CancellationToken cancellationToken = default
    )
    {
        return _context.ConfigurationParameters
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.HomologationCode == homologationCode,
                cancellationToken);
    }
}