using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.PortfolioValuations;

namespace Products.Infrastructure.PortfolioValuations;

internal sealed class PortfolioValuationConfiguration : IEntityTypeConfiguration<PortfolioValuation>
{
    public void Configure(EntityTypeBuilder<PortfolioValuation> builder)
    {
        builder.ToTable("valoracion_portafolio_dia");
        builder.HasKey(x => x.PortfolioValuationId);

        builder.Property(x => x.PortfolioValuationId).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
        builder.Property(x => x.CloseDate).HasColumnName("fecha_cierre");
        builder.Property(x => x.Value).HasColumnName("valor").HasPrecision(19, 2);
        builder.Property(x => x.Units).HasColumnName("unidades").HasPrecision(38, 16);
        builder.Property(x => x.UnitValue).HasColumnName("valor_unidad").HasPrecision(38, 16);
        builder.Property(x => x.GrossYieldUnits).HasColumnName("rendimiento_bruto_unidad").HasPrecision(38, 16);
        builder.Property(x => x.UnitCost).HasColumnName("costo_unidad").HasPrecision(38, 16);
        builder.Property(x => x.DailyYield).HasColumnName("rentabilidad_diaria").HasPrecision(38, 16);
        builder.Property(x => x.IncomingOperations).HasColumnName("operaciones_entrada").HasPrecision(19, 2);
        builder.Property(x => x.OutgoingOperations).HasColumnName("operaciones_salida").HasPrecision(19, 2);
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
    }
}