using Activations.Domain.Affiliates;
using Activations.Domain.MeetsPensionRequirements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activations.Infrastructure.MeetsPensionRequirements;
internal sealed class MeetsPensionRequirementConfiguration : IEntityTypeConfiguration<MeetsPensionRequirement>
{
    public void Configure(EntityTypeBuilder<MeetsPensionRequirement> builder)
    {
        builder.HasKey(x => x.MeetsPensionRequirementId);
        builder.Property(x => x.AffiliateId);
        builder.Property(x => x.StartDate);
        builder.Property(x => x.ExpirationDate);
        builder.Property(x => x.CreationDate);
        builder.Property(x => x.State);
    }
}