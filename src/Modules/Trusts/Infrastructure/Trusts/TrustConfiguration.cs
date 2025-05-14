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
        builder.Property(x => x.TrustId).HasColumnName("fideicomiso_id");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.ClientId).HasColumnName("cliente_id");
        builder.Property(x => x.CreationDate).HasColumnName("fecha_creacion");
        builder.Property(x => x.ObjectiveId).HasColumnName("objetivo_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.TotalBalance).HasColumnName("saldo_total");
        builder.Property(x => x.TotalUnits).HasColumnName("unidades_totales");
        builder.Property(x => x.Principal).HasColumnName("capital");
        builder.Property(x => x.Earnings).HasColumnName("rendimiento");
        builder.Property(x => x.TaxCondition).HasColumnName("condicion_tributaria");
        builder.Property(x => x.ContingentWithholding).HasColumnName("retencion_contingente");
        builder.Property(x => x.EarningsWithholding).HasColumnName("retencion_rendimiento");
        builder.Property(x => x.AvailableAmount).HasColumnName("disponible");
        builder.Property(x => x.ContingentWithholdingPercentage).HasColumnName("porcentaje_retencion_contingente");
    }
}