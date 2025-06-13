using Associate.Domain.Activates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Associate.Infrastructure.Activates;

internal sealed class ActivateConfiguration : IEntityTypeConfiguration<Activate>
{
    public void Configure(EntityTypeBuilder<Activate> builder)
    {
        builder.ToTable("activacion_afiliados");
        builder.HasKey(x => x.ActivateId);
        builder.Property(x => x.ActivateId).HasColumnName("id");
        builder.Property(x => x.DocumentType).HasColumnName("tipo_documento_uuid");
        builder.Property(x => x.Identification).HasColumnName("identificacion");
        builder.Property(x => x.Pensioner).HasColumnName("pensionado");
        builder.Property(x => x.MeetsPensionRequirements).HasColumnName("cumple_requisitos_pension");
        builder.Property(x => x.ActivateDate).HasColumnName("fecha_activacion");
    }
}