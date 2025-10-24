using System.Reflection;
using System.Text.Json;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.YieldDetails;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.YieldDetails;
using Closing.test.UnitTests.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Closing.test.UnitTests.Infrastructure;

public class YieldDetailRepositoryTests
{
    private static readonly InMemoryDatabaseRoot _dbRoot = new();

    private static DbContextOptions<ClosingDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<ClosingDbContext>()
            .UseInMemoryDatabase(dbName, _dbRoot)
            .ReplaceService<IModelCustomizer, TestModelCustomizer>() 
            .EnableSensitiveDataLogging()
            .Options;

    private sealed class TestDbContextFactory : IDbContextFactory<ClosingDbContext>
    {
        private readonly DbContextOptions<ClosingDbContext> _options;
        public TestDbContextFactory(DbContextOptions<ClosingDbContext> options) => _options = options;

        public ClosingDbContext CreateDbContext() => new ClosingDbContext(_options);

#if NET8_0_OR_GREATER
        public Task<ClosingDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
#endif
    }

    private static (ClosingDbContext ctx, IDbContextFactory<ClosingDbContext> factory) CreateCtxAndFactory()
    {
        var dbName = $"YieldDetailRepoTests-{Guid.NewGuid()}";
        var options = CreateOptions(dbName);
        var ctx = new ClosingDbContext(options);
        ctx.Database.EnsureCreated();
        var factory = new TestDbContextFactory(options);
        return (ctx, factory);
    }

    private static YieldDetail NewYield(
        long id,
        int portfolioId,
        DateTime closingDateUtc,
        bool isClosed,
        string source,
        string concept = "TEST_CONCEPT")
    {
        var type = typeof(YieldDetail);
        var inst = (YieldDetail)Activator.CreateInstance(type, nonPublic: true)!;

        void Set(string name, object? value)
            => type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                   ?.SetValue(inst, value);

        Set("YieldDetailId", id);
        Set("PortfolioId", portfolioId);
        Set("ClosingDate", closingDateUtc);
        Set("IsClosed", isClosed);
        Set("Source", source);

        SetJsonIfPropertyExists(inst, "Concept", concept);

        return inst;
    }

    private static void SetJsonIfPropertyExists(object instance, string propertyName, string value)
    {
        var type = instance.GetType();
        var pi = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pi is null) return;

        if (pi.PropertyType == typeof(JsonDocument))
        {
            var json = JsonDocument.Parse($"\"{value}\"", new JsonDocumentOptions());
            pi.SetValue(instance, json);
        }
        else
        {
            pi.SetValue(instance, value);
        }
    }

    [Fact]
    public async Task InsertAsyncMarksAdded()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);

        var y = NewYield(1, 10, new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Utc), false, "Manual");

        await repo.InsertAsync(y, CancellationToken.None);

        Assert.Equal(EntityState.Added, ctx.Entry(y).State);
    }

    [Fact]
    public async Task GetReadOnlyByPortfolioAndDateAsyncWhenIsClosedFalseReturnsOpenPlusClosedAutomaticOrdered()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);
        var d = new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Utc);

        var open1 = NewYield(31, 5, d, false, "Manual");
        var open2 = NewYield(33, 5, d, false, "Manual");
        var closedAuto = NewYield(32, 5, d, true, YieldsSources.AutomaticConcept);
        var closedMan = NewYield(34, 5, d, true, "Manual");
        var otherPort = NewYield(35, 6, d, false, "Manual");
        var otherDate = NewYield(36, 5, d.AddDays(1), false, "Manual");

        ctx.YieldDetails.AddRange(open1, open2, closedAuto, closedMan, otherPort, otherDate);
        await ctx.SaveChangesAsync();

        var list = await repo.GetReadOnlyByPortfolioAndDateAsync(5, d, isClosed: false, CancellationToken.None);

        var ids = list.Select(x => x.YieldDetailId).ToArray();
        Assert.Equal(new long[] { 31, 32, 33 }, ids);
        Assert.All(list, x => Assert.Equal(EntityState.Detached, ctx.Entry(x).State));
    }

    [Fact]
    public async Task GetReadOnlyByPortfolioAndDateAsyncWhenIsClosedTrueReturnsOnlyClosedRegardlessOfSource()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);
        var d = new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Utc);

        var open1 = NewYield(41, 7, d, false, "Manual");
        var closedAuto = NewYield(42, 7, d, true, YieldsSources.AutomaticConcept);
        var closedManual = NewYield(43, 7, d, true, "Manual");

        ctx.YieldDetails.AddRange(open1, closedAuto, closedManual);
        await ctx.SaveChangesAsync();

        var list = await repo.GetReadOnlyByPortfolioAndDateAsync(7, d, isClosed: true, CancellationToken.None);

        var ids = list.Select(x => x.YieldDetailId).OrderBy(x => x).ToArray();
        Assert.Equal(new long[] { 42, 43 }, ids);
        Assert.All(list, x => Assert.Equal(EntityState.Detached, ctx.Entry(x).State));
    }

    [Fact]
    public async Task ExistsByPortfolioAndDateAsyncHonorsIsClosedFlag()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);
        var d = new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Utc);

        ctx.YieldDetails.AddRange(
            NewYield(51, 9, d, false, "Manual"),
            NewYield(52, 9, d, true, "Manual")
        );
        await ctx.SaveChangesAsync();

        var existsOpen = await repo.ExistsByPortfolioAndDateAsync(9, d, isClosed: false, ct: CancellationToken.None);
        var existsClosed = await repo.ExistsByPortfolioAndDateAsync(9, d, isClosed: true, ct: CancellationToken.None);
        var existsNo = await repo.ExistsByPortfolioAndDateAsync(9, d.AddDays(1), isClosed: false, ct: CancellationToken.None);

        Assert.True(existsOpen);
        Assert.True(existsClosed);
        Assert.False(existsNo);
    }

    [Fact]
    public async Task InsertRangeImmediateAsyncReturnsZeroWhenNullOrEmpty()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);

        var zero1 = await repo.InsertRangeImmediateAsync(null!, CancellationToken.None);
        var zero2 = await repo.InsertRangeImmediateAsync(Array.Empty<YieldDetail>(), CancellationToken.None);

        Assert.Equal(0, zero1);
        Assert.Equal(0, zero2);
    }

    [Fact]
    public async Task InsertRangeImmediateAsyncPersistsItemsUsingFactoryContext()
    {
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);
        var d = new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Utc);

        var items = new[]
        {
            NewYield(61, 12, d, false, "Manual"),
            NewYield(62, 12, d, true , "Manual")
        };

        var inserted = await repo.InsertRangeImmediateAsync(items, CancellationToken.None);

        Assert.Equal(2, inserted);
        var all = ctx.YieldDetails.AsNoTracking().ToList();
        Assert.Contains(all, y => y.YieldDetailId == 61);
        Assert.Contains(all, y => y.YieldDetailId == 62);
    }

    [Fact]
    public async Task GetYieldDetailsAutConceptsAsync_ReturnsEmptyWhenNoMatchingPortfolios()
    {
        // Arrange
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);
        var closingDate = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        // Crear automatic concepts para portfolios diferentes
        ctx.YieldDetails.AddRange(
            NewYield(111, 200, closingDate, true, YieldsSources.AutomaticConcept, "AUTO_1"),
            NewYield(112, 201, closingDate, true, YieldsSources.AutomaticConcept, "AUTO_2")
        );
        await ctx.SaveChangesAsync();

        // Act - buscar portfolios que no existen
        var result = await repo.GetYieldDetailsAutConceptsAsync(new[] { 100, 101 }, closingDate, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetYieldDetailsAutConceptsAsync_HandlesEmptyPortfolioIdsList()
    {
        // Arrange
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);
        var closingDate = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        // Crear algunos datos
        ctx.YieldDetails.AddRange(
            NewYield(131, 100, closingDate, true, YieldsSources.AutomaticConcept, "AUTO_1"),
            NewYield(132, 101, closingDate, true, YieldsSources.AutomaticConcept, "AUTO_2")
        );
        await ctx.SaveChangesAsync();

        // Act - lista vacía de portfolios
        var result = await repo.GetYieldDetailsAutConceptsAsync(Array.Empty<int>(), closingDate, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetYieldDetailsAutConceptsAsync_ReturnsMixedClosedAndOpenAutomaticConcepts()
    {
        // Arrange
        var (ctx, factory) = CreateCtxAndFactory();
        var repo = new YieldDetailRepository(ctx, factory);
        var closingDate = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);
        var portfolioIds = new[] { 100, 101 };

        // Crear automatic concepts tanto cerrados como abiertos
        ctx.YieldDetails.AddRange(
            NewYield(141, 100, closingDate, true, YieldsSources.AutomaticConcept, "CLOSED_AUTO"),
            NewYield(142, 100, closingDate, false, YieldsSources.AutomaticConcept, "OPEN_AUTO"),
            NewYield(143, 101, closingDate, true, YieldsSources.AutomaticConcept, "CLOSED_AUTO_2"),
            NewYield(144, 101, closingDate, false, YieldsSources.AutomaticConcept, "OPEN_AUTO_2")
        );
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.GetYieldDetailsAutConceptsAsync(portfolioIds, closingDate, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count); // Debería retornar todos los automatic concepts independientemente del estado IsClosed

        // Verificar que se incluyen tanto cerrados como abiertos
        Assert.Contains(result, y => y.PortfolioId == 100 && y.IsClosed);
        Assert.Contains(result, y => y.PortfolioId == 100 && !y.IsClosed);
        Assert.Contains(result, y => y.PortfolioId == 101 && y.IsClosed);
        Assert.Contains(result, y => y.PortfolioId == 101 && !y.IsClosed);

        // Verificar que todos son automatic concepts
        Assert.All(result, y => Assert.Equal(YieldsSources.AutomaticConcept, y.Source));
    }
}
