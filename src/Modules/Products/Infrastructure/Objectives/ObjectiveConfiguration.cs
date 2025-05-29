using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Objectives;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;
using Products.Domain.Offices;
using Products.Domain.Cities;

namespace Products.Infrastructure.Objectives;

internal sealed class ObjectiveConfiguration : IEntityTypeConfiguration<Objective>
{
    public void Configure(EntityTypeBuilder<Objective> builder)
    {
        builder.ToTable("objetivos");
        builder.HasKey(x => x.ObjectiveId);
        builder.Property(x => x.ObjectiveId).HasColumnName("id");
        builder.Property(x => x.ObjectiveTypeId).HasColumnName("tipo_objetivo_id");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.Status).HasColumnName("estado");
        builder.Property(x => x.CreationDate).HasColumnName("fecha_creacion");
        builder.HasOne(o => o.Alternative)
            .WithMany(a => a.Objectives)
            .HasForeignKey(o => o.AlternativeId);
        builder.HasOne<Commercial>()
            .WithMany()
            .HasForeignKey(x => x.CommercialId);
        builder.HasOne<Office>()
            .WithMany()
            .HasForeignKey(x => x.OfficeId);
        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(x => x.CityId);
    }
}