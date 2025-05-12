using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.InputInfos;

namespace Trusts.Infrastructure.InputInfos;

internal sealed class InputInfoConfiguration : IEntityTypeConfiguration<InputInfo>
{
    public void Configure(EntityTypeBuilder<InputInfo> builder)
    {
        builder.HasKey(x => x.InputInfoId);
        builder.Property(x => x.OriginId);
        builder.Property(x => x.CollectionMethodId);
        builder.Property(x => x.PaymentFormId);
        builder.Property(x => x.CollectionAccount);
        builder.Property(x => x.PaymentFormDetail).HasColumnType("jsonb");
        builder.Property(x => x.CertificationStatusId);
        builder.Property(x => x.TaxConditionId);
        builder.Property(x => x.ContingentWithholding);
        builder.Property(x => x.VerifiableMedium).HasColumnType("jsonb");
        builder.Property(x => x.CollectionBank);
        builder.Property(x => x.DepositDate);
        builder.Property(x => x.SalesUser);
        builder.Property(x => x.City);
        builder.HasOne<CustomerDeal>().WithMany().HasForeignKey(x => x.CustomerDealId);
    }
}