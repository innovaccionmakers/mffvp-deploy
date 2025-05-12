using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trusts.Domain.CustomerDeals;

namespace Trusts.Infrastructure.CustomerDeals;

internal sealed class CustomerDealConfiguration : IEntityTypeConfiguration<CustomerDeal>
{
    public void Configure(EntityTypeBuilder<CustomerDeal> builder)
    {
        builder.HasKey(x => x.CustomerDealId);
        builder.Property(x => x.Date);
        builder.Property(x => x.AffiliateId);
        builder.Property(x => x.ObjectiveId);
        builder.Property(x => x.PortfolioId);
        builder.Property(x => x.ConfigurationParamId);
        builder.Property(x => x.Amount);
    }
}