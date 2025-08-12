using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasury.Domain.Issuers;

namespace Treasury.Infrastructure.Issuers;

internal sealed class IssuerConfiguration : IEntityTypeConfiguration<Issuer>
{
    public void Configure(EntityTypeBuilder<Issuer> builder)
    {
        builder.ToTable("emisor");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IssuerCode).HasColumnName("emisor");
        builder.Property(x => x.Description).HasColumnName("descripcion");
        builder.Property(x => x.Nit).HasColumnName("nit");
        builder.Property(x => x.Digit).HasColumnName("digito");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
        builder.Property(x => x.IsBank).HasColumnName("banco").HasDefaultValue(false);
    }
}