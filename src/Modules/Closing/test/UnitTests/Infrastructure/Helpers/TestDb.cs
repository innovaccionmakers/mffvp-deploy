
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Closing.test.UnitTests.Infrastructure.Helpers;

internal static class TestDb
{
    private static readonly InMemoryDatabaseRoot dbRoot = new();

    public static DbContextOptions<ClosingDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ClosingDbContext>()
            .UseInMemoryDatabase(dbName, dbRoot)
            .ReplaceService<IModelCustomizer, TestModelCustomizer>() 
            .EnableSensitiveDataLogging()
            .Options;
    }

    public static ClosingDbContext CreateContext(DbContextOptions<ClosingDbContext> options)
    {
        var ctx = new ClosingDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}