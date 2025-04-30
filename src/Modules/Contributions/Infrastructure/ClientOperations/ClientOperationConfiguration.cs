using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Contributions.Domain.ClientOperations;

namespace Contributions.Infrastructure.ClientOperations;
internal sealed class ClientOperationConfiguration : IEntityTypeConfiguration<ClientOperation>
{
    public void Configure(EntityTypeBuilder<ClientOperation> builder)
    {
        builder.HasKey(x => x.ClientOperationId);
        builder.Property(x => x.Date);
        builder.Property(x => x.AffiliateId);
        builder.Property(x => x.ObjectiveId);
        builder.Property(x => x.PortfolioId);
        builder.Property(x => x.TransactionTypeId);
        builder.Property(x => x.SubTransactionTypeId);
        builder.Property(x => x.Amount);
    }
}