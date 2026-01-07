using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Infrastructure.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Products.Domain.Administrators;

namespace Products.Infrastructure.Administrators;

internal sealed class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        builder.ToTable("administradores");
        builder.HasKey(a => a.AdministratorId);

        builder.Property(a => a.AdministratorId)
            .HasColumnName("id");

        builder.Property(a => a.Identification)
            .HasColumnName("identificacion")
            .HasMaxLength(25);

        builder.Property(a => a.IdentificationTypeId)
            .HasColumnName("tipo_identificacion");

        builder.Property(a => a.Digit)
            .HasColumnName("digito");
        
        builder.Property(a => a.Name)
            .HasColumnName("nombre")
            .HasMaxLength(100);

        builder.Property(a => a.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());

        builder.Property(a => a.EntityCode)
            .HasColumnName("codigo_entidad")
            .HasMaxLength(6);

        builder.Property(a => a.EntityType)
            .HasColumnName("tipo_entidad");

        builder.Property(a => a.SfcEntityCode)
            .HasColumnName("codigo_entidad_sfc")
            .HasMaxLength(6)
            .IsRequired();

        builder.HasMany(a => a.PensionFunds)
            .WithOne(f => f.Administrator)
            .HasForeignKey(f => f.AdministratorId);
    }
}