using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.PensionFunds;

namespace Products.Infrastructure.PensionFunds;

internal sealed class PensionFundConfiguration : IEntityTypeConfiguration<PensionFund>
{
    public void Configure(EntityTypeBuilder<PensionFund> builder)
    {
        builder.ToTable("fondos_voluntarios_pensiones");
        builder.HasKey(f => f.PensionFundId);

        builder.Property(f => f.PensionFundId)
            .HasColumnName("id");

        builder.Property(f => f.IdentificationTypeId)
            .HasColumnName("tipo_identificacion");

        builder.Property(f => f.IdentificationNumber)
            .HasColumnName("identificacion");

        builder.Property(f => f.Name)
            .HasColumnName("nombre");

        builder.Property(f => f.ShortName)
            .HasColumnName("nombre_corto");

        builder.Property(f => f.Status)
            .HasColumnName("estado");

        builder.Property(f => f.HomologatedCode)
            .HasColumnName("codigo_homologado");

        builder.HasMany(f => f.PlanFunds)
            .WithOne(pf => pf.PensionFund)
            .HasForeignKey(pf => pf.PensionFundId);
    }
}