using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Security.Domain.UserPermissions;

namespace Security.Infrastructure.UserPermissions;

internal sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("usuarios_permisos");

        builder.HasKey(up => up.Id);
        builder.Property(up => up.Id).HasColumnName("id");
        builder.Property(up => up.UserId).HasColumnName("usuario_id");
        builder.Property(up => up.PermitToken).HasColumnName("permiso");
        builder.Property(up => up.Granted).HasColumnName("concedido");
    }
}
