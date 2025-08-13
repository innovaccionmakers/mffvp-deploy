using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.OperationTypes;

namespace Operations.Infrastructure.OperationTypes;

internal sealed class OperationTypeConfiguration : IEntityTypeConfiguration<OperationType>
{
    public void Configure(EntityTypeBuilder<OperationType> builder)
    {
        builder.ToTable("tipos_operaciones");
        builder.HasKey(x => x.OperationTypeId);
        builder.Property(x => x.OperationTypeId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.CategoryId).HasColumnName("categoria");
        builder.Property(x => x.Nature)
            .HasColumnName("naturaleza")
            .HasConversion(new EnumMemberValueConverter<IncomeEgressNature>());
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.Property(x => x.External).HasColumnName("externo");
        builder.Property(x => x.Visible).HasColumnName("visible");
        builder.Property(x => x.AdditionalAttributes).HasColumnName("atributos_adicionales");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
    }
}