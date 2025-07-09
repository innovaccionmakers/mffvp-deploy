using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasury.Domain.BankAccounts;

namespace Treasury.Infrastructure.BankAccounts;

internal sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("cuenta_bancaria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.IssuerId).HasColumnName("emisor_id");
        builder.Property(x => x.AccountNumber).HasColumnName("numero_cuenta");
        builder.Property(x => x.AccountType).HasColumnName("tipo_cuenta");
        builder.Property(x => x.Observations).HasColumnName("observaciones");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");

        builder.HasOne(x => x.Issuer)
            .WithMany(x => x.BankAccounts)
            .HasForeignKey(x => x.IssuerId);
    }
}