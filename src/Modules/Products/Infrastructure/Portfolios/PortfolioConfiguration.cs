using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Portfolios;

namespace Products.Infrastructure.Portfolios;

internal sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portafolios");
        builder.HasKey(x => x.PortfolioId);
        builder.Property(x => x.PortfolioId).HasColumnName("id");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologacion");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.ShortName).HasColumnName("nombre_corto");
        builder.Property(x => x.ModalityId).HasColumnName("modalidad_id");
        builder.Property(x => x.InitialMinimumAmount).HasColumnName("monto_minimo_inicial");
        builder.HasMany(p => p.Alternatives)
            .WithOne(ap => ap.Portfolio)
            .HasForeignKey(ap => ap.PortfolioId);
    }
}