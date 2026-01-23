using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Infrastructure.Database;
using DomainClosingSheet = Reports.Domain.LoadingInfo.Closing.ClosingSheet;

namespace Reports.Infrastructure.LoadingInfo.Closing;

public sealed class ClosingSheetConfiguration : IEntityTypeConfiguration<DomainClosingSheet>
{
    public void Configure(EntityTypeBuilder<DomainClosingSheet> builder)
    {
        builder.ToTable("sabana_cierre", Schemas.Reports);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
          .HasColumnName("id")
          .ValueGeneratedOnAdd()
          .IsRequired();

        builder.Property(x => x.PortfolioId)
          .HasColumnName("portafolio_id")
          .IsRequired();

        builder.Property(x => x.ClosingDate)
            .HasColumnName("fecha")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(x => x.Contributions)
           .HasColumnName("aportes")
           .HasPrecision(19, 2)
            .IsRequired();

        builder.Property(x => x.Withdrawals)
          .HasColumnName("retiros")
          .HasPrecision(19, 2)
          .IsRequired();

        builder.Property(x => x.GrossPandL)
          .HasColumnName("pyg_bruto")
          .HasPrecision(19, 2)
          .IsRequired();

        builder.Property(x => x.Expenses)
           .HasColumnName("gastos")
           .HasPrecision(19, 2)
           .IsRequired();

        builder.Property(x => x.DailyFee)
            .HasColumnName("comision_dia")
             .HasPrecision(19, 2)
            .IsRequired();

        builder.Property(x => x.DailyCost)
             .HasColumnName("costo_dia")
             .HasPrecision(19, 2)
              .IsRequired();

        builder.Property(x => x.YieldsToCredit)
         .HasColumnName("rendimientos_abonar")
          .HasPrecision(19, 2)
         .IsRequired();

        builder.Property(x => x.GrossYieldPerUnit)
          .HasColumnName("rendimiento_bruto_unidad")
          .HasPrecision(38, 16)
           .IsRequired();

        builder.Property(x => x.CostPerUnit)
           .HasColumnName("costo_unidad")
            .HasPrecision(38, 16)
            .IsRequired();

        builder.Property(x => x.UnitValue)
          .HasColumnName("valor_unidad")
          .HasPrecision(38, 16)
          .IsRequired();

        builder.Property(x => x.Units)
          .HasColumnName("unidades")
          .HasPrecision(38, 16)
           .IsRequired();

        builder.Property(x => x.PortfolioValue)
            .HasColumnName("valor_portafolio")
            .HasPrecision(19, 2)
            .IsRequired();

        builder.Property(x => x.Participants)
            .HasColumnName("participes")
           .IsRequired();

        builder.Property(x => x.PortfolioFeePercentage)
           .HasColumnName("porcentaje_comision_portafolio")
           .HasPrecision(3, 2)
            .IsRequired();

        builder.Property(x => x.FundId)
             .HasColumnName("fondo_id")
              .IsRequired();
    }
}