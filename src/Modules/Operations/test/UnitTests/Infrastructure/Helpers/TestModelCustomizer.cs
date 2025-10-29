using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Operations.test.UnitTests.Infrastructure.Helpers;
internal sealed class TestModelCustomizer : ModelCustomizer
{
    public TestModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies) { }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        var jsonDocConverter = new ValueConverter<JsonDocument?, string?>(
            toDb => toDb == null ? null : toDb.RootElement.GetRawText(),
            fromDb => string.IsNullOrEmpty(fromDb) ? null : JsonDocument.Parse(fromDb!, new JsonDocumentOptions())
        );

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var clr = entity.ClrType;
            var jsonProps = clr.GetProperties().Where(p => p.PropertyType == typeof(JsonDocument));
            foreach (var prop in jsonProps)
            {
                modelBuilder.Entity(clr).Property(prop.Name).HasConversion(jsonDocConverter);
            }
        }
    }
}
