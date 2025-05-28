using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.AlternativePortfolios;
using Products.Domain.Alternatives;
using Products.Domain.Portfolios;

namespace Products.Infrastructure.AlternativePortfolios;

internal sealed class AlternativePortfolioConfiguration : IEntityTypeConfiguration<AlternativePortfolio>
{
    public void Configure(EntityTypeBuilder<AlternativePortfolio> builder)
    {
        builder.ToTable("alternativas_portafolios");
        builder.HasKey(x => x.AlternativePortfolioId);
        builder.Property(x => x.AlternativePortfolioId).HasColumnName("id");
        builder.Property(x => x.Status).HasColumnName("estado");
        builder.Property(x => x.IsCollector).HasColumnName("recaudador");
        builder.HasOne(ap => ap.Alternative)
            .WithMany(a => a.Portfolios)
            .HasForeignKey(ap => ap.AlternativeId);
        builder.HasOne(ap => ap.Portfolio)
            .WithMany(p => p.Alternatives)
            .HasForeignKey(ap => ap.PortfolioId);
    }
}