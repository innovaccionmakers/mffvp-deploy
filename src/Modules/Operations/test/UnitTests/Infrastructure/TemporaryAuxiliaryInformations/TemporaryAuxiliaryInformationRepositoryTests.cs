using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Infrastructure.Database;
using Operations.Infrastructure.TemporaryAuxiliaryInformations;
using Operations.test.UnitTests.Infrastructure.Helpers;
using System.Runtime.Serialization;
using System.Text.Json;


namespace Operations.test.UnitTests.Infrastructure.TemporaryAuxiliaryInformations
{
    public sealed class TemporaryAuxiliaryInformationRepositoryTests
    {
        private static readonly InMemoryDatabaseRoot dbRoot = new();

        private static OperationsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OperationsDbContext>()
                .UseInMemoryDatabase($"TemporaryAuxInfoRepoTests-{Guid.NewGuid()}", dbRoot)
                .ReplaceService<IModelCustomizer, TestModelCustomizer>()
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new OperationsDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        private static readonly string[] RequiredPropNames = new[]
            {
                "CollectionAccount",
                "PaymentMethodDetail",
                "SalesUser",
                "UserId",
                "VerifiableMedium"
            };

        private static TemporaryAuxiliaryInformation NewAux(
            long id,
            long? temporaryClientOperationId = null,
            DateTime? registrationDateUtc = null)
        {
            var entity = (TemporaryAuxiliaryInformation)Activator.CreateInstance(
                typeof(TemporaryAuxiliaryInformation),
                nonPublic: true)!;

            typeof(TemporaryAuxiliaryInformation)
                .GetProperty(nameof(TemporaryAuxiliaryInformation.TemporaryAuxiliaryInformationId))?
                .SetValue(entity, id);

            var type = typeof(TemporaryAuxiliaryInformation);

            var when = registrationDateUtc ?? DateTime.UtcNow;
            type.GetProperty("TemporaryClientOperationId")?.SetValue(entity, temporaryClientOperationId ?? 1L);
            type.GetProperty("RegistrationDate")?.SetValue(entity, when);
            type.GetProperty("ProcessDate")?.SetValue(entity, when);
            type.GetProperty("Key")?.SetValue(entity, "aux-key");
            type.GetProperty("Name")?.SetValue(entity, "aux-name");

            var emptyJson = JsonDocument.Parse("{}");
            type.GetProperty("Value")?.SetValue(entity, emptyJson);
            type.GetProperty("JsonData")?.SetValue(entity, emptyJson);
            type.GetProperty("Data")?.SetValue(entity, emptyJson);
            type.GetProperty("Metadata")?.SetValue(entity, emptyJson);

            var statusProp = type.GetProperty("Status");
            if (statusProp is not null && statusProp.CanWrite)
            {
                object? defaultStatus = statusProp.PropertyType.IsEnum
                    ? Enum.GetValues(statusProp.PropertyType).GetValue(0)
                    : null;
                statusProp.SetValue(entity, defaultStatus);
            }

            FillRequiredProps(entity, RequiredPropNames);

            return entity;
        }

        private static void FillRequiredProps(object entity, IEnumerable<string> propNames)
        {
            var t = entity.GetType();
            foreach (var name in propNames)
            {
                var p = t.GetProperty(name);
                if (p is null || !p.CanWrite) continue;

                var current = p.GetValue(entity);
                if (current is not null && !(current is string s && string.IsNullOrWhiteSpace(s)))
                    continue;

                p.SetValue(entity, SampleForType(p.PropertyType));
            }
        }

        private static object? SampleForType(Type type)
        {

            var u = Nullable.GetUnderlyingType(type) ?? type;

            if (u == typeof(string)) return "test";
            if (u == typeof(int)) return 1;
            if (u == typeof(long)) return 1L;
            if (u == typeof(decimal)) return 1m;
            if (u == typeof(double)) return 1d;
            if (u == typeof(bool)) return true;
            if (u == typeof(DateTime)) return DateTime.UtcNow;
            if (u == typeof(Guid)) return Guid.NewGuid();
            if (u == typeof(JsonDocument)) return JsonDocument.Parse("{}");
            if (u.IsEnum) return Enum.GetValues(u).GetValue(0)!;

            try
            {
                return Activator.CreateInstance(u, nonPublic: true)
                       ?? FormatterServices.GetUninitializedObject(u);
            }
            catch
            {

                return FormatterServices.GetUninitializedObject(u);
            }
        }

        [Fact]
        public void InsertMarksEntityAsAdded()
        {
            using var ctx = CreateContext();
            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var entity = NewAux(1001);
            repo.Insert(entity);

            Assert.Equal(EntityState.Added, ctx.Entry(entity).State);
        }

        [Fact]
        public void UpdateMarksEntityAsModified()
        {
            using var ctx = CreateContext();
            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var entity = NewAux(1002);
            ctx.Attach(entity);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(entity).State);

            repo.Update(entity);

            Assert.Equal(EntityState.Modified, ctx.Entry(entity).State);
        }

        [Fact]
        public void DeleteMarksEntityAsDeleted()
        {
            using var ctx = CreateContext();
            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var entity = NewAux(1003);
            ctx.Attach(entity);

            repo.Delete(entity);

            Assert.Equal(EntityState.Deleted, ctx.Entry(entity).State);
        }

        [Fact]
        public void DeleteRangeMarksEntitiesAsDeleted()
        {
            using var ctx = CreateContext();
            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var e1 = NewAux(1101);
            var e2 = NewAux(1102);
            ctx.AttachRange(e1, e2);

            repo.DeleteRange(new[] { e1, e2 });

            Assert.Equal(EntityState.Deleted, ctx.Entry(e1).State);
            Assert.Equal(EntityState.Deleted, ctx.Entry(e2).State);
        }

        [Fact]
        public async Task GetAllAsyncReturnsAll()
        {
            using var ctx = CreateContext();

            ctx.TemporaryAuxiliaryInformations.AddRange(
                NewAux(1),
                NewAux(2),
                NewAux(3)
            );
            await ctx.SaveChangesAsync();

            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var all = await repo.GetAllAsync();

            Assert.Equal(3, all.Count);
        }

        [Fact]
        public async Task GetAsyncReturnsByIdAndIsNotTracked()
        {
            using var ctx = CreateContext();

            ctx.TemporaryAuxiliaryInformations.Add(NewAux(50));
            await ctx.SaveChangesAsync();

            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var found = await repo.GetAsync(50);

            Assert.NotNull(found);
            Assert.Equal(50, found!.TemporaryAuxiliaryInformationId);
            Assert.Equal(EntityState.Detached, ctx.Entry(found).State); 
        }

        [Fact]
        public async Task GetAsyncReturnsNullWhenNotFound()
        {
            using var ctx = CreateContext();
            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var found = await repo.GetAsync(999);

            Assert.Null(found);
        }

        [Fact]
        public async Task GetByIdsAsyncReturnsOnlyRequestedOnes()
        {
            using var ctx = CreateContext();

            ctx.TemporaryAuxiliaryInformations.AddRange(
                NewAux(80),
                NewAux(81),
                NewAux(82)
            );
            await ctx.SaveChangesAsync();

            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var result = await repo.GetByIdsAsync(new long[] { 80, 82 });

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.TemporaryAuxiliaryInformationId == 80);
            Assert.Contains(result, x => x.TemporaryAuxiliaryInformationId == 82);
            Assert.DoesNotContain(result, x => x.TemporaryAuxiliaryInformationId == 81);
        }

     
        [Fact]
        public async Task GetAllAsyncReturnsEmptyWhenNone()
        {
            using var ctx = CreateContext();
            var repo = new TemporaryAuxiliaryInformationRepository(ctx);

            var all = await repo.GetAllAsync();

            Assert.Empty(all);
        }
    }
}
