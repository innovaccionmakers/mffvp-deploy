using Accounting.Domain.ConfigurationGenerals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.ConfigurationGenerals;

internal sealed class GeneralConfigurationConfiguration : IEntityTypeConfiguration<GeneralConfiguration>
{
    public void Configure(EntityTypeBuilder<GeneralConfiguration> builder)
    {
        builder.ToTable("configuraciones_generales");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id");

        builder.Property(x => x.PortfolioId)
                .HasColumnName("portafolio_id")
                .IsRequired();

        builder.Property(x => x.AccountingCode)
                .HasColumnName("codigo_contable")
                .HasMaxLength(10)
                .IsRequired();

        builder.Property(x => x.CostCenter)
                .HasColumnName("centro_costos")
                .HasMaxLength(12)
                .IsRequired();
    }
}

