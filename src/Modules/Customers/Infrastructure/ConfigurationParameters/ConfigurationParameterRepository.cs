using Microsoft.EntityFrameworkCore;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Customers.Infrastructure.Database;
using Customers.Domain.ConfigurationParameters;
using Customers.Application.Abstractions;

namespace Customers.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository :
    IConfigurationParameterRepository,
    IConfigurationParameterLookupRepository<CustomersModuleMarker>
{
    private readonly CustomersDbContext _context;

    public ConfigurationParameterRepository(CustomersDbContext context)
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

    public async Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationParameters
            .AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.HomologationCode == homologationCode
                     && p.Type == scope,
                cancellationToken);
    }
}