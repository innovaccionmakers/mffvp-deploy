using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Closing.Domain.Yields;

namespace Closing.Infrastructure.Yields;

internal sealed class YieldConfiguration : IEntityTypeConfiguration<Yield>
{
    public void Configure(EntityTypeBuilder<Yield> builder)
    {
        builder.ToTable("rendimientos");
        builder.HasKey(x => x.YieldId);
        builder.Property(x => x.YieldId).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.Income).HasColumnName("ingresos").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.Expenses).HasColumnName("gastos").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.Commissions).HasColumnName("comisiones").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.Costs).HasColumnName("costos").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.YieldToCredit).HasColumnName("rendimientos_abonar").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.ClosingDate).HasColumnName("fecha_cierre");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.IsClosed).HasColumnName("cerrado");

        builder.HasMany(x => x.YieldDetails)
            .WithOne(x => x.Yield)
            .HasForeignKey(x => x.YieldId)
            .HasPrincipalKey(x => x.YieldId);
    }
}