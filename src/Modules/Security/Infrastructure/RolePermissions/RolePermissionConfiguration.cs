using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Security.Domain.RolePermissions;

namespace Security.Infrastructure.RolePermissions;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "security");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("id");
        builder.Property(rp => rp.RolesId).HasColumnName("roles_id");
        builder.Property(rp => rp.ScopePermission).HasColumnName("scope_permission");

        builder.HasMany(rp => rp.UserRoles)
               .WithOne(ur => ur.RolePermission)
               .HasForeignKey(ur => ur.RolePermissionsId);
    }
}