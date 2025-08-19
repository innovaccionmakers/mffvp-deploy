using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            var values = new Dictionary<string, object?>();
            foreach (var property in entry.Properties)
            {
                values[property.Metadata.Name] = entry.OriginalValues[property.Metadata.Name];
            }
            _provider.AddState(entry.Metadata.ClrType.Name, values);
        }
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