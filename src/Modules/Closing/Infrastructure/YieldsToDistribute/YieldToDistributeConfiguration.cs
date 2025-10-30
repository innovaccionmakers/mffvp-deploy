
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Closing.Infrastructure.YieldsToDistribute;

internal sealed class YieldToDistributeConfiguration : IEntityTypeConfiguration<Domain.YieldsToDistribute.YieldToDistribute>
{
    public void Configure(EntityTypeBuilder<Domain.YieldsToDistribute.YieldToDistribute> builder)
    {
        builder.ToTable("rendimientos_por_distribuir");

        builder.HasKey(x => x.YieldToDistributeId);

        builder.Property(x => x.YieldToDistributeId)
            .HasColumnName("id");

        builder.Property(x => x.TrustId)
            .HasColumnName("fideicomiso_id");

        builder.Property(x => x.PortfolioId)
            .HasColumnName("portafolio_id");

        builder.Property(x => x.ClosingDate)
            .HasColumnName("fecha_cierre");

        builder.Property(x => x.ApplicationDate)
            .HasColumnName("fecha_aplicacion");

        builder.Property(x => x.Participation)
            .HasColumnName("participacion")
            .HasColumnType("decimal(38, 16)")
            .HasPrecision(38, 16);

        builder.Property(x => x.YieldAmount)
            .HasColumnName("rendimientos")
            .HasColumnType("decimal(19, 2)")
            .HasPrecision(19, 2);

        builder.Property(x => x.Concept)
            .HasColumnName("concepto")
            .HasColumnType("jsonb");

        builder.Property(x => x.ProcessDate)
            .HasColumnName("fecha_proceso");
    }
}
