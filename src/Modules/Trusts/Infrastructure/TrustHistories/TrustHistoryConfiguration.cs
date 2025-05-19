using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trusts.Domain.TrustHistories;
using Trusts.Domain.Trusts;

namespace Trusts.Infrastructure.TrustHistories;

internal sealed class TrustHistoryConfiguration : IEntityTypeConfiguration<TrustHistory>
{
    public void Configure(EntityTypeBuilder<TrustHistory> builder)
    {
        builder.ToTable("historicos_fideicomisos");
        builder.HasKey(x => x.TrustHistoryId);
        builder.Property(x => x.TrustHistoryId).HasColumnName("id");
        builder.Property(x => x.Earnings).HasColumnName("rendimiento");
        builder.Property(x => x.Date).HasColumnName("fecha");
        builder.Property(x => x.SalesUserId).HasColumnName("comercial_id");
        builder.HasOne<Trust>().WithMany().HasForeignKey(x => x.TrustId);
    }
}