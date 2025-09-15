using Accounting.Application.Abstractions;
using Accounting.Domain.ConfigurationParameters;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Common.SharedKernel.Domain.ConfigurationParameters;

namespace Accounting.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterRepository(AccountingDbContext context) : IConfigurationParameterRepository, IConfigurationParameterLookupRepository<AccountingModuleMarker>
{
    public async Task<ConfigurationParameter?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .SingleOrDefaultAsync(
                p => p.Uuid == uuid,
                cancellationToken
            );
    }
}
