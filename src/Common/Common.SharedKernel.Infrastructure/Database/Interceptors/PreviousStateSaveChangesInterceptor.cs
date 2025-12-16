using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Common.SharedKernel.Application.Abstractions;

namespace Common.SharedKernel.Infrastructure.Database.Interceptors;

public sealed class PreviousStateSaveChangesInterceptor(IPreviousStateProvider provider) : SaveChangesInterceptor
{
    private readonly IPreviousStateProvider _provider = provider;

    private void CaptureStates(DbContextEventData eventData)
    {
        if (eventData.Context is not DbContext context) return;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Modified or EntityState.Deleted);

        foreach (var entry in entries)
        {
            var values = new Dictionary<string, object?>();
            foreach (var property in entry.Properties)
            {
                var key = ResolveStoreKey(property);

                values[key] = property.OriginalValue;
            }
            var entityKey = entry.Metadata.ClrType.FullName ?? entry.Metadata.ClrType.Name;

            _provider.AddState(entityKey, values);
        }
    }

    private static string ResolveStoreKey(PropertyEntry propertyEntry)
    {
        try
        {
            var declaringType = propertyEntry.Metadata.DeclaringEntityType;

            var tableId = StoreObjectIdentifier.Create(declaringType, StoreObjectType.Table);
            if (tableId.HasValue)
            {
                var columnName = propertyEntry.Metadata.GetColumnName(tableId.Value);
                if (!string.IsNullOrEmpty(columnName))
                {
                    return columnName;
                }
            }

            var viewId = StoreObjectIdentifier.Create(declaringType, StoreObjectType.View);
            if (viewId.HasValue)
            {
                var columnName = propertyEntry.Metadata.GetColumnName(viewId.Value);
                if (!string.IsNullOrEmpty(columnName))
                {
                    return columnName;
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Se pasa el nombre de propiedad CLR cuando no se pueden resolver los metadatos relacionales.
        }

        return propertyEntry.Metadata.Name;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureStates(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        CaptureStates(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}