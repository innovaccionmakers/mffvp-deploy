
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Operations.Domain.TemporaryClientOperations;
using Operations.Infrastructure.Database;
using Operations.Infrastructure.TemporaryClientOperations;
using Operations.test.UnitTests.Infrastructure.Helpers;

namespace Operations.test.UnitTests.Infrastructure.TemporaryClientOperations
{
  
    public sealed class TemporaryClientOperationRepositoryTests
    {
        private static readonly InMemoryDatabaseRoot dbRoot = new();
        private static OperationsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OperationsDbContext>()
                .UseInMemoryDatabase($"TemporaryClientOpRepoTests-{Guid.NewGuid()}", dbRoot)
                .ReplaceService<IModelCustomizer, TestModelCustomizer>()
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new OperationsDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }


        private static TemporaryClientOperation NewTempOp(
            long id,
            int portfolioId,
            DateTime registrationDateUtc,
            bool processed)
        {
            var op = (TemporaryClientOperation)Activator.CreateInstance(
                typeof(TemporaryClientOperation),
                nonPublic: true)!;

            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.TemporaryClientOperationId))!
                .SetValue(op, id);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.PortfolioId))!
                .SetValue(op, portfolioId);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.RegistrationDate))!
                .SetValue(op, registrationDateUtc);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.ProcessDate))!
                .SetValue(op, registrationDateUtc);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.ApplicationDate))!
                .SetValue(op, registrationDateUtc);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.OperationTypeId))!
                .SetValue(op, 1L);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.Amount))!
                .SetValue(op, 100m);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.AffiliateId))!
                .SetValue(op, 10);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.ObjectiveId))!
                .SetValue(op, 20);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.Processed))!
                .SetValue(op, processed);

            // Campos opcionales
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.TrustId))!
                .SetValue(op, null);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.LinkedClientOperationId))!
                .SetValue(op, null);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.Units))!
                .SetValue(op, null);
            typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.CauseId))!
                .SetValue(op, null);

            var statusProp = typeof(TemporaryClientOperation).GetProperty(nameof(TemporaryClientOperation.Status));
            if (statusProp is not null && statusProp.CanWrite)
            {
                    statusProp.SetValue(op, Common.SharedKernel.Core.Primitives.LifecycleStatus.Active);
            }

            return op;
        }

        [Fact]
        public void InsertMarksEntityAsAdded()
        {
            using var ctx = CreateContext();
            using var _ = ctx;
         
            var repo = new TemporaryClientOperationRepository(ctx);

            var entity = NewTempOp(1001, 1, DateTime.UtcNow, processed: false);
            repo.Insert(entity);

            Assert.Equal(EntityState.Added, ctx.Entry(entity).State);
        }

        [Fact]
        public void UpdateMarksEntityAsModified()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            var repo = new TemporaryClientOperationRepository(ctx);

            var entity = NewTempOp(1002, 1, DateTime.UtcNow, processed: false);
            ctx.Attach(entity);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(entity).State);

            repo.Update(entity);

            Assert.Equal(EntityState.Modified, ctx.Entry(entity).State);
        }

        [Fact]
        public void DeleteMarksEntityAsDeleted()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            var repo = new TemporaryClientOperationRepository(ctx);

            var entity = NewTempOp(1003, 1, DateTime.UtcNow, processed: false);
            ctx.Attach(entity);

            repo.Delete(entity);

            Assert.Equal(EntityState.Deleted, ctx.Entry(entity).State);
        }

        [Fact]
        public async Task GetAllAsyncReturnsAll()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            ctx.TemporaryClientOperations.AddRange(
                NewTempOp(1, 10, DateTime.UtcNow, false),
                NewTempOp(2, 10, DateTime.UtcNow, true),
                NewTempOp(3, 11, DateTime.UtcNow, false)
            );
            await ctx.SaveChangesAsync();

            var repo = new TemporaryClientOperationRepository(ctx);

            var all = await repo.GetAllAsync();

            Assert.Equal(3, all.Count);
        }

        [Fact]
        public async Task GetAsyncReturnsById()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            ctx.TemporaryClientOperations.Add(NewTempOp(50, 99, DateTime.UtcNow, false));
            await ctx.SaveChangesAsync();

            var repo = new TemporaryClientOperationRepository(ctx);

            var found = await repo.GetAsync(50);

            Assert.NotNull(found);
            Assert.Equal(50, found!.TemporaryClientOperationId);
        }

        [Fact]
        public async Task GetForUpdateAsyncReturnsOnlyWhenNotProcessed()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            ctx.TemporaryClientOperations.AddRange(
                NewTempOp(60, 1, DateTime.UtcNow, processed: false),
                NewTempOp(61, 1, DateTime.UtcNow, processed: true)
            );
            await ctx.SaveChangesAsync();

            var repo = new TemporaryClientOperationRepository(ctx);

            var foundOk = await repo.GetForUpdateAsync(60);
            var foundNull = await repo.GetForUpdateAsync(61);

            Assert.NotNull(foundOk);
            Assert.Null(foundNull);
        }

        [Fact]
        public async Task GetByPortfolioAsyncFiltersUnprocessedAndOrdersByRegistrationDate()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            var t1 = NewTempOp(70, 5, new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc), processed: false);
            var t2 = NewTempOp(71, 5, new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Utc), processed: false); // más temprano
            var t3 = NewTempOp(72, 5, new DateTime(2025, 10, 1, 11, 0, 0, DateTimeKind.Utc), processed: true);  // excluido
            var t4 = NewTempOp(73, 6, new DateTime(2025, 10, 1, 8, 0, 0, DateTimeKind.Utc), processed: false); // otro portafolio

            ctx.TemporaryClientOperations.AddRange(t1, t2, t3, t4);
            await ctx.SaveChangesAsync();

            var repo = new TemporaryClientOperationRepository(ctx);

            var result = await repo.GetByPortfolioAsync(5);

            Assert.Equal(2, result.Count);
            Assert.Collection(result,
                first => Assert.Equal(71, first.TemporaryClientOperationId), 
                second => Assert.Equal(70, second.TemporaryClientOperationId)
            );
        }

        [Fact]
        public async Task GetByIdsAsyncReturnsOnlyRequestedOnes()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            ctx.TemporaryClientOperations.AddRange(
                NewTempOp(80, 1, DateTime.UtcNow, false),
                NewTempOp(81, 1, DateTime.UtcNow, false),
                NewTempOp(82, 1, DateTime.UtcNow, false)
            );
            await ctx.SaveChangesAsync();

            var repo = new TemporaryClientOperationRepository(ctx);

            var result = await repo.GetByIdsAsync(new long[] { 80, 82 });

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.TemporaryClientOperationId == 80);
            Assert.Contains(result, x => x.TemporaryClientOperationId == 82);
        }

     

        [Fact]
        public async Task GetNextPendingIdAsyncReturnsSmallestUnprocessedForPortfolio()
        {
            using var ctx = CreateContext();
            using var _ = ctx;

            ctx.TemporaryClientOperations.AddRange(
                NewTempOp(100, 7, DateTime.UtcNow, processed: true),   
                NewTempOp(101, 7, DateTime.UtcNow, processed: false),  
                NewTempOp(99, 7, DateTime.UtcNow, processed: false),  
                NewTempOp(102, 8, DateTime.UtcNow, processed: false)  
            );
            await ctx.SaveChangesAsync();

            var repo = new TemporaryClientOperationRepository(ctx);

            var nextId = await repo.GetNextPendingIdAsync(7);

            Assert.Equal(99, nextId);
        }

        [Fact]
        public async Task GetNextPendingIdAsyncReturnsNullWhenNone()
        {
            using var ctx = CreateContext();
            using var _ = ctx; ;

            ctx.TemporaryClientOperations.AddRange(
                NewTempOp(200, 3, DateTime.UtcNow, processed: true),
                NewTempOp(201, 4, DateTime.UtcNow, processed: true)
            );
            await ctx.SaveChangesAsync();

            var repo = new TemporaryClientOperationRepository(ctx);

            var nextId = await repo.GetNextPendingIdAsync(3);

            Assert.Null(nextId);
        }

      }
}
