namespace Security.Domain.UserPermissions;

public interface IUserPermissionRepository
{
    Task<IReadOnlyCollection<UserPermission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserPermission?> GetAsync(int userPermissionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserPermission>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    void Insert(UserPermission userPermission);
    void Update(UserPermission userPermission);
    void Delete(UserPermission userPermission);
}