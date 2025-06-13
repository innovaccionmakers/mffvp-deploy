using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.Channels;

namespace Operations.Infrastructure.Channels;

internal sealed class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("canales");
        builder.HasKey(x => x.ChannelId);
        builder.Property(x => x.ChannelId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
        builder.Property(x => x.System).HasColumnName("sistema");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
    }
}