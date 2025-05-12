using Activations.Domain.Affiliates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activations.Infrastructure.Affiliates;

internal sealed class AffiliateConfiguration : IEntityTypeConfiguration<Affiliate>
{
    public void Configure(EntityTypeBuilder<Affiliate> builder)
    {
        builder.HasKey(x => x.AffiliateId);
        builder.Property(x => x.IdentificationType);
        builder.Property(x => x.Identification);
        builder.Property(x => x.Pensioner);
        builder.Property(x => x.MeetsRequirements);
        builder.Property(x => x.ActivationDate);
    }
}