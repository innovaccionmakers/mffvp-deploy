using System.Reflection;
using System.Runtime.Serialization;
using Closing.Domain.Yields;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.Yields;
using Closing.test.UnitTests.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Closing.test.UnitTests.Infrastructure;

public sealed class YieldRepositoryTests
{
    private static readonly InMemoryDatabaseRoot dbRoot = new();

    private static DbContextOptions<ClosingDbContext> CreateOptions(string dbName) =>
        new DbContextOptionsBuilder<ClosingDbContext>()
            .UseInMemoryDatabase(dbName, dbRoot)
            .ReplaceService<IModelCustomizer, TestModelCustomizer>() 
            .EnableSensitiveDataLogging()
            .Options;

    private sealed class TestDbContextFactory : IDbContextFactory<ClosingDbContext>
    {
        private readonly DbContextOptions<ClosingDbContext> options;
        public TestDbContextFactory(DbContextOptions<ClosingDbContext> options) => this.options = options;

        public ClosingDbContext CreateDbContext() => new ClosingDbContext(options);

#if NET8_0_OR_GREATER
        public Task<ClosingDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
#endif
    }

    private static (ClosingDbContext ctx, IDbContextFactory<ClosingDbContext> factory) CreateCtxAndFactory()
    {
        var dbName = $"YieldRepositoryTests-{Guid.NewGuid()}";
        var options = CreateOptions(dbName);
        var ctx = new ClosingDbContext(options);
        ctx.Database.EnsureCreated();
        var factory = new TestDbContextFactory(options);
        return (ctx, factory);
    }

    private static Yield NewYield(
        long? id,
        int portfolioId,
        DateTime closingDateUtc,
        bool isClosed,
        decimal commissions = 0m,
        decimal yieldToCredit = 0m)
    {
        var instance = (Yield)FormatterServices.GetUninitializedObject(typeof(Yield));

        void Set(string name, object? value)
        {
            var t = typeof(Yield);
            var prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.SetMethod != null)
            {
                prop.SetValue(instance, value);
                return;
            }
            var backing = t.GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backing != null)
            {
                backing.SetValue(instance, value);
                return;
            }
        }

        if (id.HasValue)
        {
            Set("YieldId", id.Value);
            Set("Id", id.Value);
        }

        Set("PortfolioId", portfolioId);
        Set("ClosingDate", closingDateUtc);
        Set("IsClosed", isClosed);
        Set("Commissions", commissions);
        Set("YieldToCredit", yieldToCredit);
        Set("ProcessDate", DateTime.UtcNow);

        return instance;
    }

    [Fact]
    public async Task InsertAsyncMarksAdded()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);

        var y = NewYield(1, 10, new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc), false, 0, 0);

        await repo.InsertAsync(y, CancellationToken.None);

        Assert.Equal(EntityState.Added, ctx.Entry(y).State);
    }

    [Fact]
    public async Task SaveChangesAsyncPersistsPendingChanges()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);

        var d = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);
        var y = NewYield(2, 11, d, true, 0, 0);

        await repo.InsertAsync(y, CancellationToken.None);
        Assert.Equal(0, await ctx.Yields.CountAsync());

        await repo.SaveChangesAsync(CancellationToken.None);

        Assert.Equal(1, await ctx.Yields.CountAsync());
    }

    [Fact]
    public async Task ExistsYieldAsyncHonorsIsClosedFlag()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);
        var d = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        ctx.Yields.AddRange(
            NewYield(11, 20, d, false),
            NewYield(12, 20, d, true)
        );
        await ctx.SaveChangesAsync();

        var openExists = await repo.ExistsYieldAsync(20, d, false, CancellationToken.None);
        var closedExists = await repo.ExistsYieldAsync(20, d, true, CancellationToken.None);
        var noneExists = await repo.ExistsYieldAsync(20, d.AddDays(1), false, CancellationToken.None);

        Assert.True(openExists);
        Assert.True(closedExists);
        Assert.False(noneExists);
    }

    [Fact]
    public async Task GetForUpdateByPortfolioAndDateAsyncReturnsTrackedEntity()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);

        var stored = NewYield(21, 30, new DateTime(2025, 10, 7, 10, 30, 0, DateTimeKind.Utc), false);
        ctx.Yields.Add(stored);
        await ctx.SaveChangesAsync();

        var result = await repo.GetForUpdateByPortfolioAndDateAsync(30, new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(EntityState.Detached, ctx.Entry(result!).State);
        Assert.Equal(30, result!.PortfolioId);
    }

    [Fact]
    public async Task GetReadOnlyByPortfolioAndDateAsyncReturnsDetachedEntity()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);

        ctx.Yields.Add(NewYield(31, 40, new DateTime(2025, 10, 7, 8, 0, 0, DateTimeKind.Utc), true));
        await ctx.SaveChangesAsync();

        var result = await repo.GetReadOnlyByPortfolioAndDateAsync(40, new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(EntityState.Detached, ctx.Entry(result!).State);
    }

    [Fact]
    public async Task GetByClosingDateAsyncReturnsOneClosedPerPortfolio()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);
        var d = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        ctx.Yields.AddRange(
            NewYield(41, 50, d, true, commissions: 1, yieldToCredit: 5),
            NewYield(42, 50, d, true, commissions: 2, yieldToCredit: 6),
            NewYield(43, 51, d, false, commissions: 1, yieldToCredit: 9),
            NewYield(44, 51, d, true, commissions: 1, yieldToCredit: 9),
            NewYield(45, 52, d.AddDays(1), true)
        );
        await ctx.SaveChangesAsync();

        var list = await repo.GetByClosingDateAsync(d, CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { 50, 51 }, list.Select(x => x.PortfolioId).OrderBy(x => x).ToArray());
        Assert.All(list, x => Assert.True(x.IsClosed));
        Assert.All(list, x => Assert.Equal(d, x.ClosingDate));
    }

    [Fact]
    public async Task GetComissionsByPortfolioIdsAndClosingDateAsyncHonorsFilters()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);
        var d = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        ctx.Yields.AddRange(
            NewYield(51, 60, d, true, commissions: 10m),
            NewYield(52, 61, d, true, commissions: 0m),
            NewYield(53, 62, d, false, commissions: 7m),
            NewYield(54, 63, d.AddDays(1), true, 5m)
        );
        await ctx.SaveChangesAsync();

        var portfolios = new[] { 60, 61, 62, 63 };
        var list = await repo.GetComissionsByPortfolioIdsAndClosingDateAsync(portfolios, d, CancellationToken.None);

        Assert.Single(list);
        var item = list.First();
        Assert.Equal(60, item.PortfolioId);
        Assert.True(item.IsClosed);
        Assert.Equal(10m, item.Commissions);
    }

    [Fact]
    public async Task GetYieldsByPortfolioIdsAndClosingDateAsyncHonorsFilters()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);
        var d = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        ctx.Yields.AddRange(
            NewYield(61, 70, d, true, commissions: 0m, yieldToCredit: 100m),
            NewYield(62, 71, d, true, commissions: 0m, yieldToCredit: 0m),
            NewYield(63, 72, d, false, commissions: 0m, yieldToCredit: 5m),
            NewYield(64, 73, d.AddDays(1), true, 0m, 8m)
        );
        await ctx.SaveChangesAsync();

        var portfolios = new[] { 70, 71, 72, 73 };
        var list = await repo.GetYieldsByPortfolioIdsAndClosingDateAsync(portfolios, d, CancellationToken.None);

        Assert.Single(list);
        var item = list.First();
        Assert.Equal(70, item.PortfolioId);
        Assert.True(item.IsClosed);
        Assert.Equal(100m, item.YieldToCredit);
    }

    [Fact]
    public async Task DeleteByPortfolioAndDateAsyncThrowsOnInMemoryProvider()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);

        var d = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        ctx.Yields.AddRange(
            NewYield(71, 80, d, false),
            NewYield(72, 80, d, true),
            NewYield(73, 81, d, false)
        );
        await ctx.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repo.DeleteByPortfolioAndDateAsync(80, d, CancellationToken.None));

        Assert.Contains("ExecuteDelete", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteClosedByPortfolioAndDateAsyncThrowsOnInMemoryProvider()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldRepository(ctx, factory);

        var d = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        ctx.Yields.AddRange(
            NewYield(81, 90, d, true),
            NewYield(82, 90, d, false),
            NewYield(83, 91, d, true)
        );
        await ctx.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repo.DeleteClosedByPortfolioAndDateAsync(90, d, CancellationToken.None));

        Assert.Contains("ExecuteDelete", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
