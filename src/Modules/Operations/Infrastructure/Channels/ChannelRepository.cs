using Microsoft.EntityFrameworkCore;
using Operations.Domain.Channels;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.Channels;

internal sealed class ChannelRepository(OperationsDbContext context) : IChannelRepository
{
    public Task<Channel?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default)
    {
        return context.Channels
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.HomologatedCode == homologatedCode, cancellationToken);
    }

    public async Task<Channel?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Channels
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }
}