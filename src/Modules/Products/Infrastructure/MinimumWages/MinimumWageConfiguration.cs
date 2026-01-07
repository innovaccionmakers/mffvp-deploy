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
            builder.Property(x => x.Year).IsRequired()
                        .HasMaxLength(4)
                        .HasColumnType("character varying(4)")
                        .HasColumnName("anio");
            builder.Property(x => x.Value)
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasPrecision(19, 2)
                        .HasColumnType("numeric(19,2)")
                        .HasDefaultValue(0m)
                        .HasColumnName("valor");
        }
    }
}
