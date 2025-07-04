using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Security.Domain.RolePermissions;

namespace Security.Infrastructure.RolePermissions;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("permisos_roles");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("id");
        builder.Property(rp => rp.RolesId).HasColumnName("rol_id");
        builder.Property(rp => rp.ScopePermission).HasColumnName("permiso_ambito");

        builder.HasMany(rp => rp.UserRoles)
               .WithOne(ur => ur.RolePermission)
               .HasForeignKey(ur => ur.RolePermissionsId);
    }   
}