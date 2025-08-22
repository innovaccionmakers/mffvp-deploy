using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Infrastructure.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Products.Domain.PlanFunds;

namespace Products.Infrastructure.PlanFunds;

internal sealed class PlanFundConfiguration : IEntityTypeConfiguration<PlanFund>
{
    public void Configure(EntityTypeBuilder<PlanFund> builder)
    {
        builder.ToTable("planes_fondo");
        builder.HasKey(pf => pf.PlanFundId);

        builder.Property(pf => pf.PlanFundId)
            .HasColumnName("id");

        builder.Property(pf => pf.PlanId)
            .HasColumnName("plan_id");

        builder.Property(pf => pf.PensionFundId)
            .HasColumnName("fondo_id");

        builder.Property(pf => pf.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());

        builder.HasOne(pf => pf.Plan)
            .WithMany(p => p.PlanFunds)
            .HasForeignKey(pf => pf.PlanId);

        builder.HasOne(pf => pf.PensionFund)
            .WithMany(f => f.PlanFunds)
            .HasForeignKey(pf => pf.PensionFundId);

        builder.HasMany(pf => pf.Alternatives)
            .WithOne(a => a.PlanFund)
            .HasForeignKey(a => a.PlanFundId);
    }
}