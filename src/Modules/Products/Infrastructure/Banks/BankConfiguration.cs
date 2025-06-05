using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Banks;

namespace Products.Infrastructure.Banks;

internal sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("bancos");
        builder.HasKey(x => x.BankId);
        builder.Property(x => x.BankId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
    }
}