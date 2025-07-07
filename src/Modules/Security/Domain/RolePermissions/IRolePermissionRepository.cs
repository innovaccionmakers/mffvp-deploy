namespace Security.Domain.RolePermissions;

public interface IRolePermissionRepository
{
    Task<IReadOnlyCollection<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RolePermission?> GetAsync(int rolePermissionId, CancellationToken cancellationToken = default);
    void Insert(RolePermission rolePermission);
    void Update(RolePermission rolePermission);
    void Delete(RolePermission rolePermission);
    Task<bool> ExistsAsync(int roleId, string scopePermission, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RolePermission>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<List<string>> GetPermissionsByRoleIdsAsync(IEnumerable<int> roleIds);
}
