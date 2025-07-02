using Microsoft.EntityFrameworkCore;

using Security.Domain.UserPermissions;
using Security.Infrastructure.Database;

namespace Security.Infrastructure.UserPermissions;

internal sealed class UserPermissionRepository(SecurityDbContext context) : IUserPermissionRepository
{
    public async Task<IReadOnlyCollection<UserPermission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.UserPermissions.ToListAsync(cancellationToken);
    }

    public async Task<UserPermission?> GetAsync(int userPermissionId, CancellationToken cancellationToken = default)
    {
        return await context.UserPermissions.SingleOrDefaultAsync(x => x.Id == userPermissionId, cancellationToken);
    }

    public void Insert(UserPermission userPermission) => context.UserPermissions.Add(userPermission);
    public void Update(UserPermission userPermission) => context.UserPermissions.Update(userPermission);
    public void Delete(UserPermission userPermission) => context.UserPermissions.Remove(userPermission);
}
