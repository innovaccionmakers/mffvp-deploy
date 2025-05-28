using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Cities;

namespace Products.Infrastructure.Cities;

internal sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("ciudades");
        builder.HasKey(x => x.CityId);
        builder.Property(x => x.CityId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
    }
}