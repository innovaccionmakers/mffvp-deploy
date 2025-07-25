using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Customers.Domain.EconomicActivities;

namespace Customers.Infrastructure.EconomicActivities;

internal sealed class EconomicActivityConfiguration : IEntityTypeConfiguration<EconomicActivity>
{
    public void Configure(EntityTypeBuilder<EconomicActivity> builder)
    {
        builder.ToTable("actividades_economicas");
        builder.HasKey(x => x.EconomicActivityId);
        builder.Property(x => x.EconomicActivityId).HasColumnName("id");        
        builder.Property(x => x.GroupCode).HasColumnName("codgrupo");
        builder.Property(x => x.Description).HasColumnName("descripcion");
        builder.Property(x => x.CiiuCode).HasColumnName("codigo_ciiu");
        builder.Property(x => x.DivisionCode).HasColumnName("codigo_division");
        builder.Property(x => x.DivisionName).HasColumnName("nombre_division");
        builder.Property(x => x.GroupName).HasColumnName("nombre_grupo");
        builder.Property(x => x.ClassCode).HasColumnName("codigo_clase");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
    }
}