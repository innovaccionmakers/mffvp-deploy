using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.OriginModes;

namespace Operations.Infrastructure.OriginModes;

internal sealed class OriginModeConfiguration : IEntityTypeConfiguration<OriginMode>
{
    public void Configure(EntityTypeBuilder<OriginMode> builder)
    {
        builder.ToTable("origenaportes_modorigen");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        
        builder.Property(x => x.OriginId)
            .HasColumnName("origen_id")
            .IsRequired();

        builder.Property(x => x.ModalityOriginId)
            .HasColumnName("modalidad_origen_id")
            .IsRequired();
    }
}