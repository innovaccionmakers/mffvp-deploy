using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Security.Domain.Roles;

namespace Security.Infrastructure.Roles;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Name).HasColumnName("nombre");
        builder.Property(r => r.Objective).HasColumnName("objetivo");

        builder.HasMany(r => r.RolePermissions)
               .WithOne(rp => rp.Role)
               .HasForeignKey(rp => rp.RolesId);
    }
}
