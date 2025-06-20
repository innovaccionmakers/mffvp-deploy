using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.Banks;

namespace Operations.Infrastructure.Banks;

internal sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("bancos");
        builder.HasKey(b => b.BankId);
        builder.Property(b => b.BankId).HasColumnName("id");
        builder.Property(b => b.Nit).HasColumnName("nit").IsRequired();
        builder.Property(b => b.Name).HasColumnName("nombre").IsRequired();
        builder.Property(b => b.CompensationCode).HasColumnName("codigo_compensacion");
        builder.Property(b => b.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.Property(b => b.HomologatedCode).HasColumnName("codigo_homologado");
        builder.Property(b => b.CheckClearingDays).HasColumnName("dias_de_canje_cheques");
    }
}