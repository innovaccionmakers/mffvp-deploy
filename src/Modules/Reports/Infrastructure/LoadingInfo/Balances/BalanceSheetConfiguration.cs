using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Infrastructure.Database;
using DomainBalanceSheet = Reports.Domain.LoadingInfo.Balances.BalanceSheet;

namespace Reports.Infrastructure.LoadingInfo.Balances;

public sealed class BalanceSheetConfiguration : IEntityTypeConfiguration<DomainBalanceSheet>
{
    public void Configure(EntityTypeBuilder<DomainBalanceSheet> builder)
    {
        builder.ToTable("sabana_saldos", Schemas.Reports);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
             .ValueGeneratedOnAdd();

        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id").IsRequired();
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id").IsRequired();
        builder.Property(x => x.GoalId).HasColumnName("objetivo_id").IsRequired();

        builder.Property(x => x.Balance).HasColumnName("saldo").IsRequired();
        builder.Property(x => x.MinimumWages).HasColumnName("salarios_minimos").IsRequired();

        builder.Property(x => x.FundId).HasColumnName("fondo_id").IsRequired();

        builder.Property(x => x.GoalCreatedAtUtc).HasColumnName("fecha_creacion_objetivo").IsRequired();

        builder.Property(x => x.Age).HasColumnName("edad").IsRequired();
        builder.Property(x => x.IsDependent).HasColumnName("dependiente").IsRequired();

        builder.Property(x => x.PortfolioEntries).HasColumnName("entradas_portafolio").IsRequired();
        builder.Property(x => x.PortfolioWithdrawals).HasColumnName("salidas_portafolio").IsRequired();

        builder.Property(x => x.ClosingDateUtc).HasColumnName("fecha_cierre").IsRequired();
    }
}