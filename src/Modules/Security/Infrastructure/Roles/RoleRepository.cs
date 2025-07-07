using Microsoft.EntityFrameworkCore;

using Security.Domain.Roles;
using Security.Infrastructure.Database;

namespace Security.Infrastructure.Roles;

internal sealed class RoleRepository(SecurityDbContext context) : IRoleRepository
{
    public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles.ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await context.Roles.SingleOrDefaultAsync(x => x.Id == roleId, cancellationToken);
    }

    public void Insert(Role role) => context.Roles.Add(role);
    public void Update(Role role) => context.Roles.Update(role);
    public void Delete(Role role) => context.Roles.Remove(role);

    public async Task<bool> ExistsAsync(int roleId, CancellationToken cancellationToken)
    {
        return await context.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Id == roleId, cancellationToken);
    }
}
