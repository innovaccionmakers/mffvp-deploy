using Accounting.Domain.ConsecutiveFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.ConsecutiveFiles;

internal sealed class ConsecutiveFileConfiguration : IEntityTypeConfiguration<ConsecutiveFile>
{
    public void Configure(EntityTypeBuilder<ConsecutiveFile> builder)
    {
        builder.ToTable("consecutivos_archivo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id");

        builder.Property(x => x.GenerationDate)
                .HasColumnName("fecha_generacion")
                .IsRequired();

        builder.Property(x => x.Consecutive)
                .HasColumnName("consecutivo")
                .IsRequired();

        builder.Property(x => x.CurrentDate)
                .HasColumnName("fecha_actual")
                .IsRequired();
    }
}

