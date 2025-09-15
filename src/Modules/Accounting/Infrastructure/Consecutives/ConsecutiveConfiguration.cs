using Accounting.Domain.Consecutives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.Consecutives;

internal sealed class ConsecutiveConfiguration : IEntityTypeConfiguration<Consecutive>
{
    public void Configure(EntityTypeBuilder<Consecutive> builder)
    {
        builder.ToTable("consecutivos");

        builder.HasKey(x => x.ConsecutiveId);
        
        builder.Property(x => x.ConsecutiveId)
               .HasColumnName("id");

        builder.Property(x => x.Nature)
                .HasColumnName("naturaleza")
                .HasMaxLength(50)
                .IsRequired();

        builder.Property(x => x.SourceDocument)
                .HasColumnName("documento_fuente")
                .HasMaxLength(4)
                .IsRequired();

        builder.Property(x => x.Number)
                .HasColumnName("consecutivo")
                .IsRequired();

    }
}
