using Microsoft.EntityFrameworkCore;

using Security.Domain.UserRoles;
using Security.Infrastructure.Database;

namespace Security.Infrastructure.UserRoles;

internal sealed class UserRoleRepository(SecurityDbContext context) : IUserRoleRepository
{
    public async Task<IReadOnlyCollection<UserRole>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.UserRoles.ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetAsync(int userRoleId, CancellationToken cancellationToken = default)
    {
        return await context.UserRoles.SingleOrDefaultAsync(x => x.Id == userRoleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserRole>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public void Insert(UserRole userRole) => context.UserRoles.Add(userRole);
    public void Update(UserRole userRole) => context.UserRoles.Update(userRole);
    public void Delete(UserRole userRole) => context.UserRoles.Remove(userRole);

    public async Task<List<int>> GetRoleIdsByUserIdAsync(int userId)
    {
        return await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RolePermissionsId)
            .Distinct()
            .ToListAsync();
    }
}
