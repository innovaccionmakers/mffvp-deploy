using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Contributions.Domain.TrustOperations;
using Contributions.Domain.ClientOperations;
using Contributions.Domain.Trusts;

namespace Contributions.Infrastructure.TrustOperations;
internal sealed class TrustOperationConfiguration : IEntityTypeConfiguration<TrustOperation>
{
    public void Configure(EntityTypeBuilder<TrustOperation> builder)
    {
        builder.HasKey(x => x.TrustOperationId);
        builder.Property(x => x.Amount);
        builder.HasOne<ClientOperation>().WithMany().HasForeignKey(x => x.ClientOperationId);
        builder.HasOne<Trust>().WithMany().HasForeignKey(x => x.TrustId);
    }
}