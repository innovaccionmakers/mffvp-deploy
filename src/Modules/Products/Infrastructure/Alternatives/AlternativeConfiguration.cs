using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
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
        builder.Property(x => x.AlternativeId).HasColumnName("id");
        builder.Property(x => x.AlternativeTypeId).HasColumnName("tipo_alternativa_id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.Description).HasColumnName("descripcion");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
        builder.Property(x => x.PlanFundId)
            .HasColumnName("planes_fondo_id");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.HasOne(a => a.PlanFund)
            .WithMany(pf => pf.Alternatives)
            .HasForeignKey(a => a.PlanFundId);
        builder.HasMany(a => a.Portfolios)
            .WithOne(ap => ap.Alternative)
            .HasForeignKey(ap => ap.AlternativeId);
        builder.HasMany(a => a.Objectives)
            .WithOne(o => o.Alternative)
            .HasForeignKey(o => o.AlternativeId);
    }
}