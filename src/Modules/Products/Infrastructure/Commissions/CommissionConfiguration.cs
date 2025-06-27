using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Commissions;

namespace Products.Infrastructure.Commissions;

internal sealed class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("comisiones");
        builder.HasKey(x => x.CommissionId);

        builder.Property(x => x.CommissionId).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.Concept).HasColumnName("concepto").HasMaxLength(100);
        builder.Property(x => x.Modality).HasColumnName("modalidad").HasMaxLength(100);
        builder.Property(x => x.CommissionType).HasColumnName("tipo_comision").HasMaxLength(50);
        builder.Property(x => x.Period).HasColumnName("periodo").HasMaxLength(50);
        builder.Property(x => x.CalculationBase).HasColumnName("base_calculo").HasMaxLength(200);
        builder.Property(x => x.CalculationRule).HasColumnName("regla_calculo").HasMaxLength(2000);
        builder.Property(x => x.Status)
            .HasColumnName("activo")
            .HasConversion(new EnumMemberValueConverter<Status>());

        builder.HasMany(c => c.AccumulatedCommissions)
            .WithOne(ac => ac.Commission)
            .HasForeignKey(ac => ac.CommissionId);
    }
}