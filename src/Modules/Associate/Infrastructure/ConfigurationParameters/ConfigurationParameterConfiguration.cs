using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Associate.Domain.ConfigurationParameters;

namespace Associate.Infrastructure.ConfigurationParameters;

internal sealed class ConfigurationParameterConfiguration : IEntityTypeConfiguration<ConfigurationParameter>
{
    public void Configure(EntityTypeBuilder<ConfigurationParameter> builder)
    {
        builder.ToTable("parametros_configuracion");

        builder.HasKey(p => p.ConfigurationParameterId);
        builder.Property(p => p.ConfigurationParameterId)
               .HasColumnName("id");

        builder.Property(p => p.Uuid)
               .HasColumnName("uuid")
               .HasDefaultValueSql("uuid_generate_v7()");

        builder.Property(p => p.Name)
               .HasColumnName("nombre")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.ParentId)
               .HasColumnName("padre_id");

        builder.HasOne(p => p.Parent)
               .WithMany(p => p.Children)
               .HasForeignKey(p => p.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Status)
               .HasColumnName("estado")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(p => p.Type)
               .HasColumnName("tipo")
               .HasMaxLength(50)
               .HasDefaultValue("Generico")
               .IsRequired();

        builder.Property(p => p.Editable)
               .HasColumnName("editable")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(p => p.System)
               .HasColumnName("sistema")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(p => p.Metadata)
               .HasColumnName("metadata")
               .HasColumnType("jsonb")
               .HasDefaultValueSql("'{}'")
               .IsRequired();

        builder.Property(p => p.HomologationCode)
               .HasColumnName("codigo_homologacion")
               .HasMaxLength(20)
               .IsRequired();
    }
}