using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.TrustOperations;

namespace Operations.Infrastructure.TrustOperations;

internal sealed class TrustOperationConfiguration : IEntityTypeConfiguration<TrustOperation>
{
    public void Configure(EntityTypeBuilder<TrustOperation> builder)
    {
        builder.ToTable("operaciones_fideicomiso");
        builder.HasKey(x => x.TrustOperationId);
        builder.Property(x => x.TrustOperationId).HasColumnName("id");
        builder.Property(x => x.ClientOperationId).HasColumnName("operaciones_clientes_id");
        builder.Property(x => x.TrustId).HasColumnName("fideicomiso_id");
        builder.Property(x => x.Amount).HasColumnName("valor");
        builder.Property(x => x.Units)
            .HasColumnName("unidades")
            .HasPrecision(38, 16);
        builder.Property(x => x.OperationTypeId).HasColumnName("tipo_operaciones_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.RegistrationDate).HasColumnName("fecha_radicacion");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.ApplicationDate).HasColumnName("fecha_aplicacion");

        builder.Property(x => x.WithdrawalContingentTax).HasColumnName("retencion_contingente_retiro")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.WithdrawalContributionsTax).HasColumnName("retencion_rendimientos_retiro")
            .HasPrecision(19, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.AmountRequested).HasColumnName("valor_solicitado")
            .HasPrecision(19, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ContributionsPaid).HasColumnName("rendimientos_pagados")
            .HasPrecision(19, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.PaidCapital).HasColumnName("capital_pagado")
            .HasPrecision(19, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(x => x.ClientOperation)
            .WithMany(co => co.TrustOperations)
            .HasForeignKey(x => x.ClientOperationId)
            .IsRequired(false);

        // ===== Clave alterna para Upsert del bulk =====
        // UNIQUE(portafolio_id, fideicomiso_id, fecha_proceso, tipo_operaciones_id)
        // EFCore.BulkExtensions reconocerá esta Alternate Key como "UpdateByProperties"
        builder.HasAlternateKey(x => new { x.PortfolioId, x.TrustId, x.ProcessDate, x.OperationTypeId })
               .HasName("ux_operaciones_fideicomiso_portafolio_fideicomiso_fecha_tipo");
    }
}