namespace Security.Domain.RolePermissions;

public interface IRolePermissionRepository
{
    Task<IReadOnlyCollection<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RolePermission?> GetAsync(int rolePermissionId, CancellationToken cancellationToken = default);
    void Insert(RolePermission rolePermission);
    void Update(RolePermission rolePermission);
    void Delete(RolePermission rolePermission);
}
