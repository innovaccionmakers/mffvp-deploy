using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Security.Application.Abstractions.Data;
using Security.Domain.RolePermissions;
using Security.Domain.Roles;
using Security.Domain.UserPermissions;
using Security.Domain.UserRoles;
using Security.Domain.Users;
using Security.Domain.Logs;
using Security.Infrastructure.RolePermissions;
using Security.Infrastructure.Roles;
using Security.Infrastructure.UserPermissions;
using Security.Infrastructure.UserRoles;
using Security.Infrastructure.Users;
using Security.Infrastructure.Logs;

namespace Security.Infrastructure.Database;

public sealed class SecurityDbContext(DbContextOptions<SecurityDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<User> Users { get; set; }
    internal DbSet<Role> Roles { get; set; }
    internal DbSet<RolePermission> RolePermissions { get; set; }
    internal DbSet<UserRole> UserRoles { get; set; }
    internal DbSet<UserPermission> UserPermissions { get; set; }
    internal DbSet<Log> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Security);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new LogConfiguration());
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.CurrentTransaction.DisposeAsync();

        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
