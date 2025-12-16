using System.Text.Json;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Infrastructure.Database.Interceptors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MFFVP.IntegrationTests;

public class PreviousStateSaveChangesInterceptorTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly TestPreviousStateProvider _provider = new();
    private DbContextOptions<TestDbContext> _options = default!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new PreviousStateSaveChangesInterceptor(_provider))
            .Options;

        using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS backing_view_entities (
                Id INTEGER PRIMARY KEY,
                view_name TEXT NOT NULL
            );
            CREATE VIEW IF NOT EXISTS view_entities AS
            SELECT Id, view_name FROM backing_view_entities;
            """);
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();

    [Fact]
    public async Task Uses_column_names_for_table_mapped_entries()
    {
        await using var context = CreateContext();
        _provider.Clear();

        var entity = new TableMappedEntity
        {
            Value = "old-value",
            Secondary = "secondary"
        };

        context.TableEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.Value = "new-value";

        await context.SaveChangesAsync();

        var state = _provider.GetState(typeof(TableMappedEntity));

        Assert.Equal("old-value", state["renamed_value"]);
        Assert.Equal("secondary", state["secondary_value"]);
        Assert.True(state.ContainsKey("table_id"));
    }

    [Fact]
    public async Task Falls_back_to_property_name_when_view_mapping_is_not_resolved()
    {
        await using var context = CreateContext();
        _provider.Clear();

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO backing_view_entities (Id, view_name) VALUES (1, 'initial')");

        var entity = await context.ViewEntities.SingleAsync(e => e.Id == 1);
        entity.Name = "updated";

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

        var state = _provider.GetState(typeof(ViewMappedEntity));

        Assert.Equal("initial", state["view_name"]);
    }

    private TestDbContext CreateContext() => new(_options);

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TableMappedEntity> TableEntities => Set<TableMappedEntity>();

        public DbSet<ViewMappedEntity> ViewEntities => Set<ViewMappedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableMappedEntity>(builder =>
            {
                builder.ToTable("table_entities");
                builder.Property(e => e.Id).HasColumnName("table_id");
                builder.Property(e => e.Value).HasColumnName("renamed_value");
                builder.Property(e => e.Secondary).HasColumnName("secondary_value");
            });

            modelBuilder.Entity<ViewMappedEntity>(builder =>
            {
                builder.HasKey(e => e.Id);
                builder.ToView("view_entities");
                builder.Property(e => e.Name).HasColumnName("view_name");
            });
        }
    }

    private sealed class TableMappedEntity
    {
        public int Id { get; set; }

        public string Value { get; set; } = string.Empty;

        public string Secondary { get; set; } = string.Empty;
    }

    private sealed class ViewMappedEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestPreviousStateProvider : IPreviousStateProvider
    {
        private readonly Dictionary<string, IDictionary<string, object?>> _states = new();

        public void AddState(string entityName, IDictionary<string, object?> values)
        {
            _states[entityName] = new Dictionary<string, object?>(values);
        }

        public JsonDocument GetSerializedStateAndClear()
        {
            _states.Clear();
            return JsonDocument.Parse("{}");
        }

        public IDictionary<string, object?> GetState(Type entityType)
        {
            var key = entityType.FullName ?? entityType.Name;
            return _states[key];
        }

        public void Clear() => _states.Clear();
    }
}