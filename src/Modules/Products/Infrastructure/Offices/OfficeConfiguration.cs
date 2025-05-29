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
        builder.Property(x => x.OfficeId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
    }
}