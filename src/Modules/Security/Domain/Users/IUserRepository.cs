namespace Security.Domain.Users;

public interface IUserRepository
{
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetAsync(int userId, CancellationToken cancellationToken = default);
    void Insert(User user);
    void Update(User user);
    void Delete(User user);
}
