using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using People.Domain.Countries;

namespace People.Infrastructure.Countries;

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("paises");
        builder.HasKey(x => x.CountryId);
        builder.Property(x => x.CountryId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.ShortName).HasColumnName("nombre_abreviado");
        builder.Property(x => x.DaneCode).HasColumnName("codigo_dane");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
    }
}