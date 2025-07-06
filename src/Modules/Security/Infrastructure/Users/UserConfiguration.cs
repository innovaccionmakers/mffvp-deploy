using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Security.Domain.Users;

namespace Security.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(u => u.UserName).HasColumnName("nombre_usuario");
        builder.Property(u => u.Name).HasColumnName("nombre");
        builder.Property(u => u.MiddleName).HasColumnName("apellido");
        builder.Property(u => u.Identification).HasColumnName("identificacion");
        builder.Property(u => u.Email).HasColumnName("correo");
        builder.Property(u => u.DisplayName).HasColumnName("nombre_mostrar");

        builder.HasMany(u => u.UserRoles)
               .WithOne(ur => ur.User)
               .HasForeignKey(ur => ur.UserId);

        builder.HasMany(u => u.UserPermissions)
               .WithOne(up => up.User)
               .HasForeignKey(up => up.UserId);
    }
}
