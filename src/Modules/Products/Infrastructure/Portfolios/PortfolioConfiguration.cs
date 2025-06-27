using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Portfolios;

namespace Products.Infrastructure.Portfolios;

internal sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portafolios");
        builder.HasKey(x => x.PortfolioId);
        builder.Property(x => x.PortfolioId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.ShortName).HasColumnName("nombre_corto");
        builder.Property(x => x.ModalityId).HasColumnName("modalidad_id");
        builder.Property(x => x.InitialMinimumAmount).HasColumnName("aporte_minimo_inicial");
        builder.Property(x => x.AdditionalMinimumAmount).HasColumnName("aporte_minimo_adicional");
        builder.Property(x => x.CurrentDate).HasColumnName("fecha_actual");
        builder.Property(x => x.CommissionRateTypeId).HasColumnName("tipo_tasa_comision");
        builder.Property(x => x.CommissionPercentage).HasColumnName("porcentaje_comision");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologacion");
        builder.HasMany(p => p.Alternatives)
    .WithOne(ap => ap.Portfolio)
    .HasForeignKey(ap => ap.PortfolioId);

        builder.HasMany(p => p.Commissions)
    .WithOne(c => c.Portfolio)
    .HasForeignKey(c => c.PortfolioId);

        builder.HasMany(p => p.PortfolioValuations)
            .WithOne(pv => pv.Portfolio)
            .HasForeignKey(pv => pv.PortfolioId);
    }
}