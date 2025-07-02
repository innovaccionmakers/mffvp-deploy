namespace Security.Domain.UserRoles;

public interface IUserRoleRepository
{
    Task<IReadOnlyCollection<UserRole>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserRole?> GetAsync(int userRoleId, CancellationToken cancellationToken = default);
    void Insert(UserRole userRole);
    void Update(UserRole userRole);
    void Delete(UserRole userRole);
}
