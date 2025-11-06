using Accounting.Domain.AccountingAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.AccountingAccounts
{
    internal sealed class AccountingAccountConfiguration : IEntityTypeConfiguration<AccountingAccount>
    {
        public void Configure(EntityTypeBuilder<AccountingAccount> builder)
        {
            builder.ToTable("cuentas_contables");
            builder.HasKey(x => x.AccountingAccountId);
            builder.Property(x => x.AccountingAccountId).HasColumnName("id");
            builder.Property(x => x.Account).HasColumnName("cuenta").IsRequired().HasMaxLength(20);
            builder.Property(x => x.Name).HasColumnName("nombre").IsRequired().HasMaxLength(50);
        }
    }
}
