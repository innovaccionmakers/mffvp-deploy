namespace Security.Domain.Roles;

public interface IRoleRepository
{
    Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetAsync(int roleId, CancellationToken cancellationToken = default);
    void Insert(Role role);
    void Update(Role role);
    void Delete(Role role);
}
