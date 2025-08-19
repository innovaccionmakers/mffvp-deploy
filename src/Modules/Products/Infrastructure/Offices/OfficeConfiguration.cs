using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Offices;

namespace Products.Infrastructure.Offices;

internal sealed class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {
        builder.ToTable("oficinas");
        builder.HasKey(x => x.OfficeId);
        builder.Property(o => o.OfficeId)
            .HasColumnName("id");

        builder.Property(o => o.Name)
            .HasColumnName("nombre");

        builder.Property(o => o.Prefix)
            .HasColumnName("prefijo");
        
        builder.Property(o => o.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());

        builder.Property(o => o.HomologatedCode)
            .HasColumnName("codigo_homologado");
        
        builder.Property(o => o.CityId)
            .HasColumnName("ciudad_id");
        
        builder.Property(o => o.CostCenter)
            .HasColumnName("centro_costos");
    }
}