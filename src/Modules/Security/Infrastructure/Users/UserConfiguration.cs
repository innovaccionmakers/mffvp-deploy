using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Security.Domain.Users;

namespace Security.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user", "security");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.UserName).HasColumnName("user_name");
        builder.Property(u => u.Name).HasColumnName("name");
        builder.Property(u => u.MiddleName).HasColumnName("middle_name");
        builder.Property(u => u.Identification).HasColumnName("identification");
        builder.Property(u => u.Email).HasColumnName("email");
        builder.Property(u => u.DisplayName).HasColumnName("display_name");

        builder.HasMany(u => u.UserRoles)
               .WithOne(ur => ur.User)
               .HasForeignKey(ur => ur.UserId);

        builder.HasMany(u => u.UserPermissions)
               .WithOne(up => up.User)
               .HasForeignKey(up => up.UserId);
    }
}
