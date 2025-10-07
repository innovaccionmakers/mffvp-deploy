
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.test.UnitTests.Infrastructure.Helpers;
internal sealed class TestDbContextFactory : IDbContextFactory<ClosingDbContext>
{
    private readonly DbContextOptions<ClosingDbContext> options;

    public TestDbContextFactory(DbContextOptions<ClosingDbContext> options) => this.options = options;

    public ClosingDbContext CreateDbContext() => new ClosingDbContext(options);

    public Task<ClosingDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}