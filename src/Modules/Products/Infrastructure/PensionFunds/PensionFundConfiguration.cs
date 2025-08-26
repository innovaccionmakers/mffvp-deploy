using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Infrastructure.ValueConverters;

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

        builder.Property(f => f.Identification)
            .HasColumnName("identificacion")
            .HasMaxLength(25);

        builder.Property(f => f.Digit)
            .HasColumnName("digito");

        builder.Property(f => f.Name)
            .HasColumnName("nombre");

        builder.Property(f => f.ShortName)
            .HasColumnName("nombre_corto");

        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());

        builder.Property(f => f.HomologatedCode)
            .HasColumnName("codigo_homologado");

        builder.Property(f => f.AdministratorId)
            .HasColumnName("administrador_id");

        builder.HasOne(f => f.Administrator)
            .WithMany(a => a.PensionFunds)
            .HasForeignKey(f => f.AdministratorId);

        builder.HasMany(f => f.PlanFunds)
            .WithOne(pf => pf.PensionFund)
            .HasForeignKey(pf => pf.PensionFundId);
    }
}