using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Closing.Domain.PortfolioValuations;

namespace Closing.Infrastructure.PortfolioValuations;

internal sealed class PortfolioValuationConfiguration : IEntityTypeConfiguration<PortfolioValuation>
{
    public void Configure(EntityTypeBuilder<PortfolioValuation> builder)
    {
        builder.ToTable("valoracion_portafolio");
        builder.HasKey(x => x.PortfolioValuationId);
        builder.Property(x => x.PortfolioValuationId).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.ClosingDate).HasColumnName("fecha_cierre");
        builder.Property(x => x.Amount).HasColumnName("valor").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.Units).HasColumnName("unidades").HasColumnType("decimal(38, 16)");
        builder.Property(x => x.UnitValue).HasColumnName("valor_unidad").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.GrossYieldPerUnit).HasColumnName("rendimiento_bruto_unidad").HasColumnType("decimal(38, 16)");
        builder.Property(x => x.CostPerUnit).HasColumnName("costo_unidad").HasColumnType("decimal(38, 16)");
        builder.Property(x => x.DailyProfitability).HasColumnName("rentabilidad_diaria").HasColumnType("decimal(38, 16)");
        builder.Property(x => x.IncomingOperations).HasColumnName("operaciones_entrada").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.OutgoingOperations).HasColumnName("operaciones_salida").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso").HasColumnType("date");
        builder.Property(x => x.IsClosed).HasColumnName("cerrado");
    }
}