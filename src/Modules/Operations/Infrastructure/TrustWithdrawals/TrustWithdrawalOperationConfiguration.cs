using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.TrustWithdrawals;

namespace Operations.Infrastructure.TrustWithdrawals;

internal sealed class TrustWithdrawalOperationConfiguration : IEntityTypeConfiguration<TrustWithdrawalOperation>
{
    public void Configure(EntityTypeBuilder<TrustWithdrawalOperation> builder)
    {
        builder.ToTable("operaciones_retiro_fideicomiso");
        builder.HasKey(x => x.TrustWithdrawalOperationId);
        builder.Property(x => x.TrustWithdrawalOperationId).HasColumnName("id");
        builder.Property(x => x.ClientOperationId).HasColumnName("operaciones_clientes_id");
        builder.Property(x => x.TrustId).HasColumnName("fideicomiso_id");
        builder.Property(x => x.Amount).HasColumnName("valor");

        builder.HasOne(x => x.ClientOperation)
            .WithMany(co => co.TrustWithdrawals)
            .HasForeignKey(x => x.ClientOperationId);
    }
}