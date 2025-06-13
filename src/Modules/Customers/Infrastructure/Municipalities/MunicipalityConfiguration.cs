using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Customers.Domain.Municipalities;

namespace Customers.Infrastructure.Municipalities;

internal sealed class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure(EntityTypeBuilder<Municipality> builder)
    {
        builder.ToTable("municipios");
        builder.HasKey(x => x.MunicipalityId);
        builder.Property(x => x.MunicipalityId).HasColumnName("id");
        builder.Property(x => x.CityCode).HasColumnName("codigo_municipio");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.DialingCode).HasColumnName("indicativo");
        builder.Property(x => x.DaneCode).HasColumnName("codigo_dane");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
    }
}