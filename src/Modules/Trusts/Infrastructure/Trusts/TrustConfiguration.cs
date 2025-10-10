using Common.SharedKernel.Core.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trusts.Domain.Trusts;

namespace Trusts.Infrastructure.Trusts;

internal sealed class TrustConfiguration : IEntityTypeConfiguration<Trust>
{
    public void Configure(EntityTypeBuilder<Trust> builder)
    {
        builder.ToTable("fideicomisos");
        builder.HasKey(x => x.TrustId);
        builder.Property(x => x.TrustId).HasColumnName("id");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.ClientOperationId).HasColumnName("operaciones_cliente_id");
        builder.Property(x => x.CreationDate).HasColumnName("fecha_creacion");
        builder.Property(x => x.ObjectiveId).HasColumnName("objetivo_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.TotalBalance)
            .HasColumnName("saldo_total")
            .HasColumnType("decimal(19, 2)")
            .HasPrecision(19,2);
        builder.Property(x => x.TotalUnits)
            .HasColumnName("unidades_totales")
            .HasColumnType("decimal(38, 16)")
             .HasPrecision(38, 16);
        builder.Property(x => x.Principal)
            .HasColumnName("capital")
            .HasColumnType("decimal(19, 2)")
             .HasPrecision(19, 2);
        builder.Property(x => x.Earnings)
            .HasColumnName("rendimiento")
            .HasColumnType("decimal(19, 2)")
             .HasPrecision(19, 2);
        builder.Property(x => x.TaxCondition).HasColumnName("condicion_tributaria");
        builder.Property(x => x.ContingentWithholding)
            .HasColumnName("retencion_contingente")
            .HasColumnType("decimal(19, 2)")
            .HasPrecision(19, 2);
        builder.Property(x => x.EarningsWithholding)
            .HasColumnName("retencion_rendimiento")
            .HasColumnType("decimal(19, 2)")
            .HasPrecision(19, 2);
        builder.Property(x => x.AvailableAmount)
            .HasColumnName("disponible")
            .HasColumnType("decimal(19, 2)")
            .HasPrecision(19, 2);
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion<int>();
        builder.Property(x => x.UpdateDate)
            .HasColumnName("fecha_actualizacion");
    }
}
