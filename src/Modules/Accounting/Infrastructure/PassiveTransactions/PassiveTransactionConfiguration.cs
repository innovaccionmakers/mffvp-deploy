using Accounting.Domain.PassiveTransactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.PassiveTransactions;

internal class PassiveTransactionConfiguration : IEntityTypeConfiguration<PassiveTransaction>
{
    public void Configure(EntityTypeBuilder<PassiveTransaction> builder)
    {
        builder.ToTable("transacciones_pasivas");

        builder.HasKey(x => x.PassiveTransactionId);
        builder.Property(x => x.PassiveTransactionId)
               .HasColumnName("id");

        builder.Property(x => x.PortfolioId)
                .HasColumnName("portafolio_id")
                .IsRequired();

        builder.Property(x => x.TypeOperationsId)
                .HasColumnName("tipo_operaciones_id")
                .HasColumnType("bigint")
                .IsRequired();

        builder.Property(x => x.DebitAccount)
                .HasColumnName("cuenta_debito")
                .HasMaxLength(20);

        builder.Property(x => x.CreditAccount)
                .HasColumnName("cuenta_credito")
                .HasMaxLength(20);

        builder.Property(x => x.ContraCreditAccount)
                .HasColumnName("cuenta_contra_credito")
                .HasMaxLength(20);

        builder.Property(x => x.ContraDebitAccount)
                .HasColumnName("cuenta_contra_debito")
                .HasMaxLength(20);
    }
}
