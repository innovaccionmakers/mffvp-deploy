using Microsoft.EntityFrameworkCore;

using Security.Domain.Users;
using Security.Infrastructure.Database;

namespace Security.Infrastructure.Users;

internal sealed class UserRepository(SecurityDbContext context) : IUserRepository
{
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await context.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public void Insert(User user) => context.Users.Add(user);
    public void Update(User user) => context.Users.Update(user);
    public void Delete(User user) => context.Users.Remove(user);
}
