using Accounting.Domain.Concepts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.Concepts;

internal sealed class ConceptConfiguration : IEntityTypeConfiguration<Concept>
{
    public void Configure(EntityTypeBuilder<Concept> builder)
    {
        builder.ToTable("conceptos");

        builder.HasKey(x => x.ConceptId);
        builder.Property(x => x.ConceptId)
               .HasColumnName("id");

        builder.Property(x => x.PortfolioId)
                .HasColumnName("portafolio_id")
                .IsRequired();

        builder.Property(x => x.Name)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

        builder.Property(x => x.DebitAccount)
                .HasColumnName("cuenta_debito")
                .HasMaxLength(20);

        builder.Property(x => x.CreditAccount)
                .HasColumnName("cuenta_credito")
                .HasMaxLength(20);
    }
}
