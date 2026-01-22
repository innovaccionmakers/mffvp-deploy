using Common.SharedKernel.Domain.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common.SharedKernel.Infrastructure.Database.Interceptors;

public sealed class RowVersionUpdatedInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context is null)
            return result;

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var entry in context.ChangeTracker.Entries<IHasRowVersion>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.RowVersion = now;
            }
        }

        return result;
    }
}