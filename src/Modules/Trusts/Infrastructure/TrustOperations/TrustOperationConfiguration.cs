using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.TrustOperations;
using Trusts.Domain.Trusts;

namespace Trusts.Infrastructure.TrustOperations;

internal sealed class TrustOperationConfiguration : IEntityTypeConfiguration<TrustOperation>
{
    public void Configure(EntityTypeBuilder<TrustOperation> builder)
    {
        builder.HasKey(x => x.TrustOperationId);
        builder.Property(x => x.Amount);
        builder.HasOne<CustomerDeal>().WithMany().HasForeignKey(x => x.CustomerDealId);
        builder.HasOne<Trust>().WithMany().HasForeignKey(x => x.TrustId);
    }
}