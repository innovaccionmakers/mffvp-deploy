using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Closing.Domain.TrustYields;

namespace Closing.Infrastructure.TrustYields;

internal sealed class TrustYieldConfiguration : IEntityTypeConfiguration<TrustYield>
{
    public void Configure(EntityTypeBuilder<TrustYield> builder)
    {
        builder.ToTable("rendimientos_fideicomisos");
        builder.HasKey(x => x.TrustYieldId);
        builder.Property(x => x.TrustYieldId)
                .HasColumnName("id");

        builder.Property(x => x.TrustId)
                .HasColumnName("fideicomiso_id");

        builder.Property(x => x.PortfolioId)
                .HasColumnName("portafolio_id");

        builder.Property(x => x.ClosingDate)
                .HasColumnName("fecha_cierre")
                 .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Participation)
                .HasColumnName("participacion")
                .HasColumnType("decimal(38, 16)")
                .HasPrecision(38, 16);

        builder.Property(x => x.Units)
                .HasColumnName("unidades")
                .HasColumnType("decimal(38, 16)")
                .HasPrecision(38, 16);

        builder.Property(x => x.YieldAmount)
                .HasColumnName("rendimientos")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.PreClosingBalance)
                .HasColumnName("saldo_precierre")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.ClosingBalance)
                .HasColumnName("saldo_cierre")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.Income)
                .HasColumnName("ingresos")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.Expenses)
                .HasColumnName("gastos")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.Commissions)
                .HasColumnName("comisiones")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.Cost)
                .HasColumnName("costo")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.Capital)
                .HasColumnName("capital")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);
            
        builder.Property(x => x.ProcessDate)
            .HasColumnName("fecha_proceso");

        builder.Property(x => x.ContingentRetention)
                .HasColumnName("retencion_contingente")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        builder.Property(x => x.YieldRetention)
                .HasColumnName("retencion_rendimiento")
                .HasColumnType("decimal(19, 2)")
                .HasPrecision(19, 2);

        // Constraint que usa el bulk (como CONSTRAINT, no sólo índice)
        // La librería BulkExtensions de EF Core requiere que la clave alterna esté definida como CONSTRAINT (no lo reconoce como index) en la base de datos.
        builder.HasAlternateKey(x => new { x.PortfolioId, x.TrustId, x.ClosingDate })
               .HasName("ux_rendimientos_fideicomisos_portafolio_fideicomiso_fecha");
    }
}