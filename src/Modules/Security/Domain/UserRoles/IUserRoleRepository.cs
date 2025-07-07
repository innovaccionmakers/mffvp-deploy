namespace Security.Domain.UserRoles;

public interface IUserRoleRepository
{
    Task<IReadOnlyCollection<UserRole>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserRole?> GetAsync(int userRoleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserRole>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    void Insert(UserRole userRole);
    void Update(UserRole userRole);
    void Delete(UserRole userRole);
    Task<List<int>> GetRoleIdsByUserIdAsync(int userId);
}
