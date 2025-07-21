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
        builder.Property(x => x.PortfolioValuationId)
               .HasColumnName("id");

        builder.Property(x => x.PortfolioId)
               .HasColumnName("portafolio_id");

        builder.Property(x => x.ClosingDate)
               .HasColumnName("fecha_cierre");

        builder.Property(x => x.Amount)
               .HasColumnName("valor")
               .HasPrecision(19, 2);

        builder.Property(x => x.Units)
               .HasColumnName("unidades")
               .HasPrecision(38, 16);

        builder.Property(x => x.UnitValue)
               .HasColumnName("valor_unidad")
               .HasPrecision(38, 16);

        builder.Property(x => x.GrossYieldPerUnit)
               .HasColumnName("rendimiento_bruto_unidad")
               .HasPrecision(38, 16);

        builder.Property(x => x.CostPerUnit)
               .HasColumnName("costo_unidad")
               .HasPrecision(38, 16);

        builder.Property(x => x.DailyProfitability)
               .HasColumnName("rentabilidad_diaria")
               .HasPrecision(38, 16);

        builder.Property(x => x.IncomingOperations)
               .HasColumnName("operaciones_entrada")
               .HasPrecision(19, 2);

        builder.Property(x => x.OutgoingOperations)
               .HasColumnName("operaciones_salida")
               .HasPrecision(19, 2);

        builder.Property(x => x.ProcessDate)
               .HasColumnName("fecha_proceso")
               .HasColumnType("date");

        builder.Property(x => x.IsClosed)
               .HasColumnName("cerrado");
    }
}