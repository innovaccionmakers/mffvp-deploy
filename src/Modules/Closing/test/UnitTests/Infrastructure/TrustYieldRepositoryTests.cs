using Closing.Domain.TrustYields;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.TrustYields;
using Closing.test.UnitTests.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using System.Text.Json;

namespace Closing.test.UnitTests.Infrastructure;

public class TrustYieldRepositoryTests
{
    private static readonly InMemoryDatabaseRoot InMemRoot = new();

    private static ClosingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ClosingDbContext>()
            .UseInMemoryDatabase($"TrustYieldRepoTests-{Guid.NewGuid()}", InMemRoot)
            .ReplaceService<IModelCustomizer, TestModelCustomizer>() 
            .EnableSensitiveDataLogging()
            .Options;

        var ctx = new ClosingDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static TrustYield NewTrustYield(
        long trustYieldId,
        long trustId,
        int portfolioId,
        DateTime closingDateUtc)
    {
        var t = typeof(TrustYield);
        var inst = (TrustYield)Activator.CreateInstance(t, nonPublic: true)!;

        void Set(string name, object? value)
            => t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.SetValue(inst, value);

        Set("TrustYieldId", trustYieldId);
        Set("TrustId", trustId);
        Set("PortfolioId", portfolioId);
        Set("ClosingDate", closingDateUtc);

        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (p.SetMethod == null) continue;
            if (p.Name is "TrustYieldId" or "TrustId" or "PortfolioId" or "ClosingDate") continue;

            if (p.PropertyType == typeof(string)) p.SetValue(inst, "TEST");
            else if (p.PropertyType == typeof(decimal)) p.SetValue(inst, 0m);
            else if (p.PropertyType == typeof(int)) p.SetValue(inst, 0);
            else if (p.PropertyType == typeof(bool)) p.SetValue(inst, false);
            else if (p.PropertyType == typeof(DateTime)) p.SetValue(inst, closingDateUtc);
            else if (p.PropertyType == typeof(JsonDocument))
                p.SetValue(inst, JsonDocument.Parse("\"TEST\"", new JsonDocumentOptions()));
        }

        return inst;
    }

    [Fact]
    public async Task InsertAsyncMarksAdded()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var e = NewTrustYield(1, trustId: 1001, portfolioId: 1, closingDateUtc: DateTime.UtcNow.Date);

        await repo.InsertAsync(e, CancellationToken.None);

        Assert.Equal(EntityState.Added, ctx.Entry(e).State);
    }

    [Fact]
    public void UpdateMarksModified()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var e = NewTrustYield(2, 1002, 2, DateTime.UtcNow.Date);
        ctx.Attach(e);
        Assert.Equal(EntityState.Unchanged, ctx.Entry(e).State);

        repo.Update(e);

        Assert.Equal(EntityState.Modified, ctx.Entry(e).State);
    }

    [Fact]
    public async Task GetReadOnlyByTrustAndDateAsyncReturnsEntityDetachedOrNull()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var d = new DateTime(2025, 10, 06, 0, 0, 0, DateTimeKind.Utc);
        var yes = NewTrustYield(10, 2001, portfolioId: 5, d);
        var no1 = NewTrustYield(11, 2001, portfolioId: 5, d.AddDays(1));
        var no2 = NewTrustYield(12, 2002, portfolioId: 5, d);

        ctx.TrustYields.AddRange(yes, no1, no2);
        await ctx.SaveChangesAsync();

        var found = await repo.GetReadOnlyByTrustAndDateAsync(2001, d, CancellationToken.None);
        Assert.NotNull(found);
        Assert.Equal(EntityState.Detached, ctx.Entry(found!).State);

        var notFound = await repo.GetReadOnlyByTrustAndDateAsync(9999, d, CancellationToken.None);
        Assert.Null(notFound);
    }

    [Fact]
    public async Task GetForUpdateByPortfolioAndDateAsyncReturnsTrackedList()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var d = new DateTime(2025, 10, 06, 0, 0, 0, DateTimeKind.Utc);

        var a = NewTrustYield(20, 3001, 7, d);
        var b = NewTrustYield(21, 3002, 7, d);
        var c = NewTrustYield(22, 3003, 8, d);  
        var e = NewTrustYield(23, 3004, 7, d.AddDays(1));

        ctx.TrustYields.AddRange(a, b, c, e);
        await ctx.SaveChangesAsync();

        var list = await repo.GetForUpdateByPortfolioAndDateAsync(7, d, CancellationToken.None);
        Assert.Equal(2, list.Count);
        Assert.All(list, x => Assert.NotEqual(EntityState.Detached, ctx.Entry(x).State));
    }

    [Fact]
    public async Task GetReadOnlyByPortfolioAndDateAsyncReturnsDetachedList()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var d = new DateTime(2025, 10, 06, 0, 0, 0, DateTimeKind.Utc);
        ctx.TrustYields.AddRange(
            NewTrustYield(30, 4001, 9, d),
            NewTrustYield(31, 4002, 9, d),
            NewTrustYield(32, 4003, 9, d.AddDays(1)),
            NewTrustYield(33, 4004, 10, d)
        );
        await ctx.SaveChangesAsync();

        var list = await repo.GetReadOnlyByPortfolioAndDateAsync(9, d, CancellationToken.None);
        Assert.Equal(2, list.Count);
        Assert.All(list, x => Assert.Equal(EntityState.Detached, ctx.Entry(x).State));
    }

    [Fact]
    public async Task GetTrustIdsByPortfolioAsyncGroupsByPortfolioForDate()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var d = new DateTime(2025, 10, 06, 0, 0, 0, DateTimeKind.Utc);

        ctx.TrustYields.AddRange(
            NewTrustYield(40, 5001, 11, d),
            NewTrustYield(41, 5002, 11, d),
            NewTrustYield(42, 5003, 12, d),
            NewTrustYield(43, 5004, 12, d),
            NewTrustYield(44, 5005, 12, d)
        );

        ctx.TrustYields.Add(NewTrustYield(45, 9999, 11, d.AddDays(1)));

        await ctx.SaveChangesAsync();

        var result = await repo.GetTrustIdsByPortfolioAsync(d, CancellationToken.None);

        var byPortfolio = result.ToDictionary(x => x.PortfolioId, x => x.TrustIds);

        Assert.True(byPortfolio.ContainsKey(11));
        Assert.True(byPortfolio.ContainsKey(12));
        Assert.Equal(new[] { 5001L, 5002L }, byPortfolio[11].OrderBy(x => x).ToArray());
        Assert.Equal(new[] { 5003L, 5004L, 5005L }, byPortfolio[12].OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task SaveChangesAsyncPersistsInserted()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var d = DateTime.UtcNow.Date;
        var e1 = NewTrustYield(60, 7001, 15, d);
        var e2 = NewTrustYield(61, 7002, 15, d);

        await repo.InsertAsync(e1, CancellationToken.None);
        await repo.InsertAsync(e2, CancellationToken.None);

        await repo.SaveChangesAsync(CancellationToken.None);

        var all = await ctx.TrustYields.AsNoTracking().OrderBy(x => x.TrustYieldId).ToListAsync();
        Assert.Equal(new[] { 60L, 61L }, all.Select(x => x.TrustYieldId).ToArray());
    }

    [Fact]
    public async Task GetReadOnlyByTrustIdsAndDateAsyncReturnsDictionaryFilteredByDate()
    {
        using var ctx = CreateContext();
        var repo = new TrustYieldRepository(ctx);

        var d = new DateTime(2025, 10, 06, 0, 0, 0, DateTimeKind.Utc);
        ctx.TrustYields.AddRange(
            NewTrustYield(70, 8001, 16, d),
            NewTrustYield(71, 8002, 16, d),
            NewTrustYield(72, 8003, 16, d.AddDays(1)) 
        );
        await ctx.SaveChangesAsync();

        var dict = await repo.GetReadOnlyByTrustIdsAndDateAsync(new long[] { 8001, 8002, 9999 }, d, CancellationToken.None);

        Assert.Equal(2, dict.Count);
        Assert.True(dict.ContainsKey(8001));
        Assert.True(dict.ContainsKey(8002));
        Assert.False(dict.ContainsKey(9999));

        Assert.Equal(EntityState.Detached, ctx.Entry(dict[8001]).State);
        Assert.Equal(EntityState.Detached, ctx.Entry(dict[8002]).State);
    }
}
