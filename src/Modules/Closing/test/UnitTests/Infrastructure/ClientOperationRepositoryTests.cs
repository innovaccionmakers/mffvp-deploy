﻿
using Closing.Domain.ClientOperations;
using Closing.Infrastructure.ClientOperations;
using Closing.Infrastructure.Database;
using Closing.test.UnitTests.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
namespace Closing.test.UnitTests.Infrastructure;


public class ClientOperationRepositoryTests
{
    private static readonly InMemoryDatabaseRoot dbRoot = new();

    private static ClosingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ClosingDbContext>()
            .UseInMemoryDatabase($"ClientOpRepoTests-{Guid.NewGuid()}", dbRoot)
            .ReplaceService<IModelCustomizer, TestModelCustomizer>() 
            .EnableSensitiveDataLogging()
            .Options;

        var ctx = new ClosingDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static ClientOperation NewOp(
        long id,
        int portfolioId,
        DateTime processDateUtc,
        long operationTypeId,
        decimal amount)
    {

        var op = (ClientOperation)Activator.CreateInstance(typeof(ClientOperation), nonPublic: true)!;

        typeof(ClientOperation).GetProperty("ClientOperationId")?.SetValue(op, id);
        typeof(ClientOperation).GetProperty("PortfolioId")?.SetValue(op, portfolioId);
        typeof(ClientOperation).GetProperty("ProcessDate")?.SetValue(op, processDateUtc);
        typeof(ClientOperation).GetProperty("OperationTypeId")?.SetValue(op, operationTypeId);
        typeof(ClientOperation).GetProperty("Amount")?.SetValue(op, amount);

        return op;
    }

    [Fact]
    public void InsertMarksEntityAsAdded()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var entity = NewOp(101, 1, DateTime.UtcNow, 10, 123.45m);
        repo.Insert(entity);

        Assert.Equal(EntityState.Added, ctx.Entry(entity).State);
    }

    [Fact]
    public void UpdateMarksEntityAsModified()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var entity = NewOp(102, 2, DateTime.UtcNow, 11, 50m);
        ctx.Attach(entity);
        Assert.Equal(EntityState.Unchanged, ctx.Entry(entity).State);

        repo.Update(entity);

        Assert.Equal(EntityState.Modified, ctx.Entry(entity).State);
    }

    [Fact]
    public async Task ClientOperationsExistsAsyncReturnsTrueWhenExactMatch()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var date = new DateTime(2025, 10, 6, 13, 0, 0, DateTimeKind.Utc);

        ctx.ClientOperations.AddRange(
            NewOp(201, 2, date, 99, 10m),                
            NewOp(202, 2, date.AddMinutes(1), 99, 10m),  
            NewOp(203, 3, date, 99, 10m)                
        );
        await ctx.SaveChangesAsync();

        var exists = await repo.ClientOperationsExistsAsync(2, date, 99, CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task ClientOperationsExistsAsyncReturnsFalseWhenNoExactMatch()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var date = new DateTime(2025, 10, 6, 13, 0, 0, DateTimeKind.Utc);

        ctx.ClientOperations.AddRange(
            NewOp(211, 2, date.AddSeconds(1), 99, 10m), 
            NewOp(212, 2, date, 100, 10m),            
            NewOp(213, 9, date, 99, 10m) 
        );
        await ctx.SaveChangesAsync();

        var exists = await repo.ClientOperationsExistsAsync(2, date, 99, CancellationToken.None);

        Assert.False(exists);
    }

    [Fact]
    public async Task SumByPortfolioAndSubtypesAsyncSumsOnlyMatchingDateAndTypes()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var date = new DateTime(2025, 10, 6, 13, 0, 0, DateTimeKind.Utc);
        var sameDayLater = date.AddHours(5);
        var nextDay = date.AddDays(1);

        ctx.ClientOperations.AddRange(
            NewOp(301, 5, date, 1, 10m),
            NewOp(302, 5, sameDayLater, 2, 5.5m), 
            NewOp(303, 5, nextDay, 1, 99m),     
            NewOp(304, 5, date, 3, 100m),         
            NewOp(305, 6, date, 1, 777m)         
        );
        await ctx.SaveChangesAsync();

        var subtypes = new long[] { 1, 2 };

        var total = await repo.SumByPortfolioAndSubtypesAsync(5, date, subtypes, CancellationToken.None);

        Assert.Equal(15.5m, total);
    }

    [Fact]
    public async Task SumByPortfolioAndSubtypesAsyncReturnsZeroWhenNoMatches()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var date = new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Utc);

        ctx.ClientOperations.AddRange(
            NewOp(311, 1, date.AddDays(1), 1, 10m),
            NewOp(312, 1, date, 3, 20m)
        );
        await ctx.SaveChangesAsync();

        var total = await repo.SumByPortfolioAndSubtypesAsync(1, date, new long[] { 2 }, CancellationToken.None);

        Assert.Equal(0m, total);
    }

    [Fact]
    public async Task GetForUpdateByIdAsyncReturnsEntityWithoutTracking()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var date = new DateTime(2025, 10, 6, 10, 0, 0, DateTimeKind.Utc);
        var existing = NewOp(401, 77, date, 8, 300m);
        ctx.ClientOperations.Add(existing);
        await ctx.SaveChangesAsync();

        var found = await repo.GetForUpdateByIdAsync(401, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(EntityState.Detached, ctx.Entry(found!).State);
    }

    [Fact]
    public async Task GetForUpdateByIdAsyncReturnsNullWhenNotFound()
    {
        using var ctx = CreateContext();
        var repo = new ClientOperationRepository(ctx);

        var found = await repo.GetForUpdateByIdAsync(999999, CancellationToken.None);

        Assert.Null(found);
    }
}