using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.MinimumWages;

namespace Products.Infrastructure.MinimumWages
{
    internal sealed class MinimumWageConfiguration : IEntityTypeConfiguration<MinimumWage>
    {
        public void Configure(EntityTypeBuilder<MinimumWage> builder)
        {
            builder.ToTable("salarios_minimos");
            builder.HasKey(x => x.MinimumWageId);
            builder.Property(x => x.MinimumWageId).HasColumnName("id");
            builder.Property(x => x.Year).IsRequired().HasColumnName("anio");
            builder.Property(x => x.Value).IsRequired().HasColumnName("valor");
        }
    }
}
