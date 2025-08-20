using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Infrastructure.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Operations.Domain.Origins;

namespace Operations.Infrastructure.Origins;

internal sealed class OriginConfiguration : IEntityTypeConfiguration<Origin>
{
    public void Configure(EntityTypeBuilder<Origin> builder)
    {
        builder.ToTable("origen_aportes");
        builder.HasKey(x => x.OriginId);
        builder.Property(x => x.OriginId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.OriginatorMandatory).HasColumnName("obligatoriedad_originador");
        builder.Property(x => x.RequiresCertification).HasColumnName("exige_certificacion");
        builder.Property(x => x.RequiresContingentWithholding).HasColumnName("exige_retencion_contingente");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
        builder.HasMany(x => x.AuxiliaryInformations)
            .WithOne(ai => ai.Origin)
            .HasForeignKey(ai => ai.OriginId);
        builder.HasMany(o => o.OriginModes)
            .WithOne(om => om.Origin)
            .HasForeignKey(om => om.OriginId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}