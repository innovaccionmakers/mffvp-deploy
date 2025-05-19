using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Alternatives;

namespace Products.Infrastructure.Alternatives;

internal sealed class AlternativeConfiguration : IEntityTypeConfiguration<Alternative>
{
    public void Configure(EntityTypeBuilder<Alternative> builder)
    {
        builder.ToTable("alternativas");
        builder.HasKey(x => x.AlternativeId);
        builder.Property(x => x.AlternativeId).HasColumnName("alternativa_id");
        builder.Property(x => x.AlternativeTypeId).HasColumnName("tipo_alternativa_id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.Status).HasColumnName("estado");
        builder.Property(x => x.Description).HasColumnName("descripcion");
    }
}