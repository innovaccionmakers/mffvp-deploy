using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.TechnicalSheets;

namespace Products.Infrastructure.TechnicalSheets;

internal sealed class TechnicalSheetConfiguration : IEntityTypeConfiguration<TechnicalSheet>
{
    public void Configure(EntityTypeBuilder<TechnicalSheet> builder)
    {
        builder.ToTable("ficha_tecnica");

        builder.HasKey(x => x.TechnicalSheetId);
        builder.Property(x => x.TechnicalSheetId)
               .HasColumnName("id");

        builder.Property(x => x.PortfolioId)
               .HasColumnName("portafolio_id");

        builder.Property(x => x.Date)
               .HasColumnName("fecha")
               .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Contributions)
               .HasColumnName("aportes")
               .HasPrecision(19, 2);

        builder.Property(x => x.Withdrawals)
               .HasColumnName("retiros")
               .HasPrecision(19, 2);

        builder.Property(x => x.GrossPnl)
               .HasColumnName("pyg_bruto")
               .HasPrecision(19, 2);

        builder.Property(x => x.Expenses)
               .HasColumnName("gastos")
               .HasPrecision(19, 2);

        builder.Property(x => x.DailyCommission)
               .HasColumnName("comision_dia")
               .HasPrecision(19, 2);

        builder.Property(x => x.DailyCost)
               .HasColumnName("costo_dia")
               .HasPrecision(19, 2);

        builder.Property(x => x.CreditedYields)
               .HasColumnName("rendimientos_abonados")
               .HasPrecision(19, 2);

        builder.Property(x => x.GrossUnitYield)
               .HasColumnName("rendimiento_bruto_unidad")
               .HasPrecision(38, 16);

        builder.Property(x => x.UnitCost)
               .HasColumnName("costo_unidad")
               .HasPrecision(38, 16);

        builder.Property(x => x.UnitValue)
               .HasColumnName("valor_unidad")
               .HasPrecision(38, 16);

        builder.Property(x => x.Units)
               .HasColumnName("unidades")
               .HasPrecision(38, 16);

        builder.Property(x => x.PortfolioValue)
               .HasColumnName("valor_portafolio")
               .HasPrecision(19, 2);

        builder.Property(x => x.Participants)
               .HasColumnName("participes");

        // Relationships
        builder.HasOne(x => x.Portfolio)
               .WithMany()
               .HasForeignKey(x => x.PortfolioId);

        // Indexes
        builder.HasIndex(x => x.PortfolioId)
               .HasDatabaseName("ix_ficha_tecnica_portafolio_id");

        builder.HasIndex(x => x.Date)
               .HasDatabaseName("ix_ficha_tecnica_fecha");
    }
}