
using Operations.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Operations.test.UnitTests.Infrastructure.Helpers;

internal static class TestDb
{
    private static readonly InMemoryDatabaseRoot dbRoot = new();

    public static DbContextOptions<OperationsDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<OperationsDbContext>()
            .UseInMemoryDatabase(dbName, dbRoot)
            .ReplaceService<IModelCustomizer, TestModelCustomizer>() 
            .EnableSensitiveDataLogging()
            .Options;
    }

    public static OperationsDbContext CreateContext(DbContextOptions<OperationsDbContext> options)
    {
        var ctx = new OperationsDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}