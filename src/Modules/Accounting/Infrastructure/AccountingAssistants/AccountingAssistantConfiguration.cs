using Accounting.Domain.AccountingAssistants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.AccountingAssistants;

internal sealed class AccountingAssistantConfiguration : IEntityTypeConfiguration<AccountingAssistant>
{
    public void Configure(EntityTypeBuilder<AccountingAssistant> builder)
    {
        builder.ToTable("auxiliar_contable");

        builder.HasKey(x => x.AccountingAssistantId);
        builder.Property(x => x.AccountingAssistantId)
            .HasColumnName("id");

        builder.Property(x => x.Identification)
            .HasColumnName("identificacion")
            .HasMaxLength(13)
            .IsRequired();


        builder.Property(x => x.VerificationDigit)
            .HasColumnName("digito_verificacion");

        builder.Property(x => x.Name)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Period)
            .HasColumnName("periodo")
            .HasMaxLength(6);

        builder.Property(x => x.Account)
            .HasColumnName("cuenta")
            .HasMaxLength(6);

        builder.Property(x => x.Date)
            .HasColumnName("fecha")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Detail)
            .HasColumnName("detalle")
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .HasColumnName("tipo")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnName("valor")
            .HasPrecision(19, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(x => x.Nature)
            .HasColumnName("naturaleza")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Identifier)
            .HasColumnName("identificador")
            .HasColumnType("UUID")
            .IsRequired();
    }
}
