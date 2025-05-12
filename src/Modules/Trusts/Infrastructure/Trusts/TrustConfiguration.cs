using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trusts.Domain.Trusts;

namespace Trusts.Infrastructure.Trusts;

internal sealed class TrustConfiguration : IEntityTypeConfiguration<Trust>
{
    public void Configure(EntityTypeBuilder<Trust> builder)
    {
        builder.HasKey(x => x.TrustId);
        builder.Property(x => x.AffiliateId);
        builder.Property(x => x.ClientId);
        builder.Property(x => x.ObjectiveId);
        builder.Property(x => x.PortfolioId);
        builder.Property(x => x.TotalBalance);
        builder.Property(x => x.TotalUnits);
        builder.Property(x => x.Principal);
        builder.Property(x => x.Earnings);
        builder.Property(x => x.TaxCondition);
        builder.Property(x => x.ContingentWithholding);
    }
}