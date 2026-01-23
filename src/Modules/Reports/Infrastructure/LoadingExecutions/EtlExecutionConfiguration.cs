using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Infrastructure.Database;

namespace Reports.Infrastructure.LoadingExecutions;

public sealed class EtlExecutionConfiguration : IEntityTypeConfiguration<EtlExecution>
{
    public void Configure(EntityTypeBuilder<EtlExecution> builder)
    {
        builder.ToTable("ejecucion_reportes", Schemas.Reports);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("nombre_carga")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Parameters)
            .HasColumnName("parametros")
            .HasColumnType("jsonb");

        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.StartedAtUtc)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(x => x.FinishedAtUtc)
            .HasColumnName("fecha_fin");

        builder.Property(x => x.DurationMilliseconds)
            .HasColumnName("duracion");

        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("jsonb");
    }
}