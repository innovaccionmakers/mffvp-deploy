using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Security.Domain.UserRoles;

namespace Security.Infrastructure.UserRoles;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "security");

        builder.HasKey(ur => ur.Id);
        builder.Property(ur => ur.Id).HasColumnName("id");
        builder.Property(ur => ur.RolePermissionsId).HasColumnName("role_permissions_id");
        builder.Property(ur => ur.UserId).HasColumnName("user_id");
    }
}
