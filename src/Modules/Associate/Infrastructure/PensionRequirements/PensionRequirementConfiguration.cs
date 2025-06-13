using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Associate.Domain.PensionRequirements;
using Associate.Domain.Activates;

namespace Associate.Infrastructure.PensionRequirements;
internal sealed class PensionRequirementConfiguration : IEntityTypeConfiguration<PensionRequirement>
{
    public void Configure(EntityTypeBuilder<PensionRequirement> builder)
    {
        builder.ToTable("requisitos_pension");
        builder.HasKey(x => x.PensionRequirementId);
        builder.Property(x => x.PensionRequirementId).HasColumnName("id");
        builder.Property(x => x.StartDate).HasColumnName("fecha_inicio");
        builder.Property(x => x.ExpirationDate).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.CreationDate).HasColumnName("fecha_creacion");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.HasOne<Activate>().WithMany().HasForeignKey(x => x.ActivateId);
        builder.Property(x => x.ActivateId).HasColumnName("afiliado_id");
    }
}