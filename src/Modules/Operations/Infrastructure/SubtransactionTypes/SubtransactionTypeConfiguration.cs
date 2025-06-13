using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.SubtransactionTypes;

namespace Operations.Infrastructure.SubtransactionTypes;

internal sealed class SubtransactionTypeConfiguration : IEntityTypeConfiguration<SubtransactionType>
{
    public void Configure(EntityTypeBuilder<SubtransactionType> builder)
    {
        builder.ToTable("subtipo_transacciones");
        builder.HasKey(x => x.SubtransactionTypeId);
        builder.Property(x => x.SubtransactionTypeId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.Category).HasColumnName("categoria");
        builder.Property(x => x.Nature).HasColumnName("naturaleza");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.Property(x => x.External).HasColumnName("externo");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
    }
}