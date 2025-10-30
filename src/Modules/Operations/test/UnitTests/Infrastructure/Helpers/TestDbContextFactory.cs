
using Operations.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Operations.test.UnitTests.Infrastructure.Helpers;
internal sealed class TestDbContextFactory : IDbContextFactory<OperationsDbContext>
{
    private readonly DbContextOptions<OperationsDbContext> options;

    public TestDbContextFactory(DbContextOptions<OperationsDbContext> options) => this.options = options;

    public OperationsDbContext CreateDbContext() => new OperationsDbContext(options);

    public Task<OperationsDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}