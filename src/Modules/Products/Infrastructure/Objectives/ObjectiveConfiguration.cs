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
        builder.Property(o => o.ObjectiveId)
            .HasColumnName("id");

        builder.Property(o => o.ObjectiveTypeId)
            .HasColumnName("tipo_objetivo_id");

        builder.Property(o => o.AffiliateId)
            .HasColumnName("afiliado_id");

        builder.Property(o => o.AlternativeId)
            .HasColumnName("alternativa_id");

        builder.Property(o => o.Name)
            .HasColumnName("nombre");

        builder.Property(o => o.Status)
            .HasColumnName("estado");

        builder.Property(o => o.CreationDate)
            .HasColumnName("fecha_creacion");

        builder.Property(o => o.Balance)
            .HasColumnName("saldo")
            .HasPrecision(19, 2);

        builder.Property(o => o.CommercialId)
            .HasColumnName("comercial_id");

        builder.Property(o => o.OpeningOfficeId)
            .HasColumnName("oficina_apertura_id");

        builder.Property(o => o.CurrentOfficeId)
            .HasColumnName("oficina_actual_id");

        builder.HasOne(o => o.Alternative)
            .WithMany(a => a.Objectives)
            .HasForeignKey(o => o.AlternativeId);

        builder.HasOne(o => o.Commercial)
            .WithMany()
            .HasForeignKey(o => o.CommercialId);
    }
}