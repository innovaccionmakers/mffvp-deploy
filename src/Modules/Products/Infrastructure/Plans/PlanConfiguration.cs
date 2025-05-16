
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Plans;

namespace Products.Infrastructure.Plans;
internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("planes");  
        builder.HasKey(x => x.PlanId);
        builder.Property(x => x.PlanId).HasColumnName("plan_id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.Description).HasColumnName("descripcion");
    }
}