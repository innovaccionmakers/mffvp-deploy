using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Closing.Domain.YieldDetails;

namespace Closing.Infrastructure.YieldDetails;

internal sealed class YieldDetailConfiguration : IEntityTypeConfiguration<YieldDetail>
{
    public void Configure(EntityTypeBuilder<YieldDetail> builder)
    {
        builder.ToTable("detalle_rendimientos");
        builder.HasKey(x => x.YieldDetailId);
        builder.Property(x => x.YieldDetailId).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.ClosingDate).HasColumnName("fecha_cierre");
        builder.Property(x => x.Source).HasColumnName("fuente").HasMaxLength(50);
        builder.Property(x => x.Concept).HasColumnName("concepto").HasColumnType("jsonb");
        builder.Property(x => x.Income).HasColumnName("ingresos").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.Expenses).HasColumnName("gastos").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.Commissions).HasColumnName("comisiones").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.IsClosed).HasColumnName("cerrado");
    }
}