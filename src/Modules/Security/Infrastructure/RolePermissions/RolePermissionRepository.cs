using Microsoft.EntityFrameworkCore;

using Security.Domain.RolePermissions;
using Security.Infrastructure.Database;

namespace Security.Infrastructure.RolePermissions;

internal sealed class RolePermissionRepository(SecurityDbContext context) : IRolePermissionRepository
{
    public async Task<IReadOnlyCollection<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions.ToListAsync(cancellationToken);
    }

    public async Task<RolePermission?> GetAsync(int rolePermissionId, CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions.SingleOrDefaultAsync(x => x.Id == rolePermissionId, cancellationToken);
    }

    public void Insert(RolePermission rolePermission) => context.RolePermissions.Add(rolePermission);
    public void Update(RolePermission rolePermission) => context.RolePermissions.Update(rolePermission);
    public void Delete(RolePermission rolePermission) => context.RolePermissions.Remove(rolePermission);

    public async Task<bool> ExistsAsync(int roleId, string scopePermission, CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.ScopePermission == scopePermission, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.ScopePermission)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetPermissionsByRoleIdsAsync(IEnumerable<int> roleIds)
    {
        return await context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.ScopePermission)
            .Distinct()
            .ToListAsync();
    }
}
