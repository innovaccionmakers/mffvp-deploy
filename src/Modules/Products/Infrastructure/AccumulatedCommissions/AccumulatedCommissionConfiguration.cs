using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.AccumulatedCommissions;

namespace Products.Infrastructure.AccumulatedCommissions;

internal sealed class AccumulatedCommissionConfiguration : IEntityTypeConfiguration<AccumulatedCommission>
{
    public void Configure(EntityTypeBuilder<AccumulatedCommission> builder)
    {
        builder.ToTable("comisiones_acumuladas");
        builder.HasKey(x => x.AccumulatedCommissionId);

        builder.Property(x => x.AccumulatedCommissionId).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
        builder.Property(x => x.CommissionId).HasColumnName("comisiones_id");
        builder.Property(x => x.AccumulatedValue).HasColumnName("valor_acumulado").HasPrecision(19, 2);
        builder.Property(x => x.PaidValue).HasColumnName("valor_pagado").HasPrecision(19, 2);
        builder.Property(x => x.PendingValue).HasColumnName("valor_pendiente").HasPrecision(19, 2);
        builder.Property(x => x.CloseDate).HasColumnName("fecha_cierre");
        builder.Property(x => x.PaymentDate).HasColumnName("fecha_pago");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
    }
}