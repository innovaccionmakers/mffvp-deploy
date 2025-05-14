
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Objectives;

namespace Products.Infrastructure.Objectives;
internal sealed class ObjectiveConfiguration : IEntityTypeConfiguration<Objective>
{
    public void Configure(EntityTypeBuilder<Objective> builder)
    {
        builder.ToTable("objetivos");  
        builder.HasKey(x => x.ObjectiveId);
        builder.Property(x => x.ObjectiveId).HasColumnName("objetivo_id");
        builder.Property(x => x.ObjectiveTypeId).HasColumnName("tipo_objetivo_id");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.AlternativeId).HasColumnName("alternativa_id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.Status).HasColumnName("estado");
        builder.Property(x => x.CreationDate).HasColumnName("fecha_creacion");
    }
}