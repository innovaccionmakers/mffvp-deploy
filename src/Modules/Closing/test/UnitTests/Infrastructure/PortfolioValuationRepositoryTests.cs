
using System.Reflection;
using System.Runtime.Serialization;
using Closing.Domain.PortfolioValuations;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.PortfolioValuations;
using Closing.test.UnitTests.Infrastructure.Helpers; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; 
using Microsoft.EntityFrameworkCore.Storage;

namespace Closing.test.UnitTests.Infrastructure;

public sealed class PortfolioValuationRepositoryTests
{
    private static readonly InMemoryDatabaseRoot dbRoot = new();

    private static DbContextOptions<ClosingDbContext> CreateOptions(string dbName) =>
        new DbContextOptionsBuilder<ClosingDbContext>()
            .UseInMemoryDatabase(dbName, dbRoot)
            .ReplaceService<IModelCustomizer, TestModelCustomizer>() 
            .EnableSensitiveDataLogging()
            .Options;

    private static ClosingDbContext CreateCtx()
    {
        var dbName = $"PortfolioValuationRepoTests-{Guid.NewGuid()}";
        var options = CreateOptions(dbName);
        var ctx = new ClosingDbContext(options); 
        ctx.Database.EnsureCreated();
        return ctx;
    }

    // ==== Helpers ====

    private static PortfolioValuation NewValuation(
        int portfolioId,
        DateTime closingDateUtc,
        bool isClosed,
        long? id = null)
    {
        var instance = (PortfolioValuation)FormatterServices.GetUninitializedObject(typeof(PortfolioValuation));

        void SetIfExists(string name, object? value)
        {
            var t = typeof(PortfolioValuation);
            var prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.SetMethod != null)
            {
                prop.SetValue(instance, value);
                return;
            }
            var backing = t.GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backing != null)
                backing.SetValue(instance, value);
        }

        SetIfExists("PortfolioId", portfolioId);
        SetIfExists("ClosingDate", closingDateUtc);
        SetIfExists("IsClosed", isClosed);

        if (id.HasValue)
        {
            SetIfExists("PortfolioValuationId", id.Value);
            SetIfExists("Id", id.Value);
        }

        SetIfExists("ProcessDate", DateTime.UtcNow);
        SetIfExists("Value", 0m);
        SetIfExists("UnitValue", 1m);
        SetIfExists("Units", 0m);

        return instance;
    }

    [Fact]
    public async Task GetReadOnlyByPortfolioAndDateAsyncReturnsNullWhenNotFound()
    {
        await using var ctx = CreateCtx();
        var repo = new PortfolioValuationRepository(ctx);

        var result = await repo.GetReadOnlyByPortfolioAndDateAsync(999, DateTime.UtcNow.Date, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReadOnlyByPortfolioAndDateAsyncReturnsClosedMatchOnly()
    {
        var date = DateTime.UtcNow.Date;

        await using (var seed = CreateCtx())
        {
            seed.PortfolioValuations.AddRange(
                NewValuation(1, date, true, 1),
                NewValuation(1, date, false, 2),
                NewValuation(2, date, true, 3),
                NewValuation(1, date.AddDays(1), true, 4)
            );
            await seed.SaveChangesAsync();
        }

        await using var ctx = CreateCtx();
        var repo = new PortfolioValuationRepository(ctx);

        ctx.PortfolioValuations.AddRange(
            NewValuation(1, date, true, 10),
            NewValuation(1, date, false, 11),
            NewValuation(2, date, true, 12),
            NewValuation(1, date.AddDays(1), true, 13)
        );
        await ctx.SaveChangesAsync();

        var result = await repo.GetReadOnlyByPortfolioAndDateAsync(1, date, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result!.PortfolioId);
        Assert.Equal(date, result.ClosingDate);
        Assert.True(result.IsClosed);
        Assert.Equal(EntityState.Detached, ctx.Entry(result).State);
    }

    [Fact]
    public async Task ExistsByPortfolioAndDateAsyncReturnsTrueOnlyWhenClosedExists()
    {
        var date = DateTime.UtcNow.Date;
        await using var ctx = CreateCtx();
        ctx.PortfolioValuations.AddRange(
            NewValuation(10, date, false, 1),
            NewValuation(10, date.AddDays(1), true, 2)
        );
        await ctx.SaveChangesAsync();

        var repo = new PortfolioValuationRepository(ctx);

        Assert.False(await repo.ExistsByPortfolioAndDateAsync(10, date, CancellationToken.None));

        ctx.PortfolioValuations.Add(NewValuation(10, date, true, 3));
        await ctx.SaveChangesAsync();

        Assert.True(await repo.ExistsByPortfolioAndDateAsync(10, date, CancellationToken.None));
    }

    [Fact]
    public async Task ExistsByPortfolioIdAsyncReturnsTrueWhenAnyRowExists()
    {
        await using var ctx = CreateCtx();
        var date = DateTime.UtcNow.Date;

        ctx.PortfolioValuations.AddRange(
            NewValuation(7, date, false, 1),
            NewValuation(7, date.AddDays(1), true, 2)
        );
        await ctx.SaveChangesAsync();

        var repo = new PortfolioValuationRepository(ctx);

        Assert.True(await repo.ExistsByPortfolioIdAsync(7, CancellationToken.None));
        Assert.False(await repo.ExistsByPortfolioIdAsync(8, CancellationToken.None));
    }

    [Fact]
    public async Task AddAsyncAddsEntityAndRequiresSaveChanges()
    {
        await using var ctx = CreateCtx();
        var repo = new PortfolioValuationRepository(ctx);

        var v = NewValuation(20, DateTime.UtcNow.Date, true);
        await repo.AddAsync(v, CancellationToken.None);

        Assert.Equal(0, await ctx.PortfolioValuations.CountAsync());

        await ctx.SaveChangesAsync();
        Assert.Equal(1, await ctx.PortfolioValuations.CountAsync());
    }

    [Fact]
    public async Task DeleteClosedByPortfolioAndDateAsyncBehavesAccordingToProvider()
    {
        var date = DateTime.UtcNow.Date;
        await using var ctx = CreateCtx();

        ctx.PortfolioValuations.AddRange(
            NewValuation(3, date, true, 1),
            NewValuation(3, date, false, 2),
            NewValuation(3, date.AddDays(1), true, 3),
            NewValuation(4, date, true, 4)
        );
        await ctx.SaveChangesAsync();

        var repo = new PortfolioValuationRepository(ctx);
        var isInMemory = ctx.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true;

        if (isInMemory)
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repo.DeleteClosedByPortfolioAndDateAsync(3, date, CancellationToken.None));

            Assert.Contains("ExecuteDelete", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            await repo.DeleteClosedByPortfolioAndDateAsync(3, date, CancellationToken.None);

            var remaining = await ctx.PortfolioValuations
                .Where(v => v.ClosingDate >= date.AddDays(-1) && v.ClosingDate <= date.AddDays(1))
                .ToListAsync();

            Assert.DoesNotContain(remaining, v => v.PortfolioId == 3 && v.ClosingDate == date && v.IsClosed);
            Assert.Contains(remaining, v => v.PortfolioId == 3 && v.ClosingDate == date && !v.IsClosed);
            Assert.Contains(remaining, v => v.PortfolioId == 3 && v.ClosingDate == date.AddDays(1) && v.IsClosed);
            Assert.Contains(remaining, v => v.PortfolioId == 4 && v.ClosingDate == date && v.IsClosed);
        }
    }

    [Fact]
    public async Task GetPortfolioValuationsByClosingDateAsyncReturnsOneClosedPerPortfolio()
    {
        var date = DateTime.UtcNow.Date;
        await using var ctx = CreateCtx();

        ctx.PortfolioValuations.AddRange(
            NewValuation(1, date, true, 1),
            NewValuation(1, date, true, 2),
            NewValuation(2, date, false, 3),
            NewValuation(2, date, true, 4),
            NewValuation(3, date.AddDays(1), true, 5)
        );
        await ctx.SaveChangesAsync();

        var repo = new PortfolioValuationRepository(ctx);
        var result = await repo.GetPortfolioValuationsByClosingDateAsync(date, CancellationToken.None);

        Assert.Equal(2, result.Count);
        var portfolios = result.Select(r => r.PortfolioId).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { 1, 2 }, portfolios);
        Assert.All(result, r => Assert.True(r.IsClosed));
        Assert.All(result, r => Assert.Equal(date, r.ClosingDate));
    }
}
