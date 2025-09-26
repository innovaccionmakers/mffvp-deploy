using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.Treasuries;

internal sealed class TreasuryConfiguration : IEntityTypeConfiguration<Domain.Treasuries.Treasury>
{
    public void Configure(EntityTypeBuilder<Domain.Treasuries.Treasury> builder)
    {
        builder.ToTable("tesoreria");

        builder.HasKey(x => x.TreasuryId);
        builder.Property(x => x.TreasuryId)
               .HasColumnName("id");

        builder.Property(x => x.PortfolioId)
               .HasColumnName("portafolio_id")
               .IsRequired();


        builder.Property(x => x.BankAccount)
                .HasColumnName("cuenta_bancaria")
                .HasMaxLength(20);     
        
        builder.Property(x => x.DebitAccount)
                .HasColumnName("cuenta_debito")
                .HasMaxLength(20);

        builder.Property(x => x.CreditAccount)
                .HasColumnName("cuenta_credito")
                .HasMaxLength(20);
    }
}
