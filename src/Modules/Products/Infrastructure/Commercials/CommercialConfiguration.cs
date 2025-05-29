using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Commercials;

namespace Products.Infrastructure.Commercials;

internal sealed class CommercialConfiguration : IEntityTypeConfiguration<Commercial>
{
    public void Configure(EntityTypeBuilder<Commercial> builder)
    {
        builder.ToTable("comerciales");
        builder.HasKey(x => x.CommercialId);
        builder.Property(x => x.CommercialId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
    }
}