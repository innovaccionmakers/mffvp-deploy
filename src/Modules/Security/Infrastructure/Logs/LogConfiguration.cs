using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Security.Domain.Logs;

namespace Security.Infrastructure.Logs;

internal sealed class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .HasColumnType("bigint");

        builder.Property(l => l.Date)
            .HasColumnName("fecha")
            .HasColumnType("timestamp with time zone");

        builder.Property(l => l.Action)
            .HasColumnName("accion")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200);

        builder.Property(l => l.User)
            .HasColumnName("usuario")
            .HasColumnType("varchar(256)")
            .HasMaxLength(256);

        builder.Property(l => l.Ip)
            .HasColumnName("ip")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(l => l.Machine)
            .HasColumnName("maquina")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(l => l.Description)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200);

        builder.Property(l => l.ObjectData)
            .HasColumnName("objeto")
            .HasColumnType("jsonb");

        builder.Property(l => l.PreviousObjectState)
            .HasColumnName("estado_anterior_objeto")
            .HasColumnType("jsonb");

        builder.Property(l => l.SuccessfulProcess)
            .HasColumnName("proceso_exitoso")
            .HasColumnType("boolean");
    }
}