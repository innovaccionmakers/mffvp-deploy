using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Closing.Domain.ProfitLosses;

namespace Closing.Infrastructure.ProfitLosses;

internal sealed class ProfitLossConfiguration : IEntityTypeConfiguration<ProfitLoss>
{
    public void Configure(EntityTypeBuilder<ProfitLoss> builder)
    {
        builder.ToTable("pyg");
        builder.HasKey(x => x.ProfitLossId);
        builder.Property(x => x.ProfitLossId).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.EffectiveDate).HasColumnName("fecha_efectiva");
        builder.Property(x => x.ProfitLossConceptId).HasColumnName("concepto_id");
        builder.Property(x => x.Amount).HasColumnName("valor").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.Source).HasColumnName("fuente");
        
        builder.HasOne(x => x.ProfitLossConcept)
            .WithMany(x => x.ProfitLosses)
            .HasForeignKey(x => x.ProfitLossConceptId);
    }
}