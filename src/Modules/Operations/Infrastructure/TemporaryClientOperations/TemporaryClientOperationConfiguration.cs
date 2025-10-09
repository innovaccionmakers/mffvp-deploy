using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.TemporaryClientOperations;

namespace Operations.Infrastructure.TemporaryClientOperations;

internal sealed class TemporaryClientOperationConfiguration : IEntityTypeConfiguration<TemporaryClientOperation>
{
    public void Configure(EntityTypeBuilder<TemporaryClientOperation> builder)
    {
        builder.ToTable("operaciones_clientes_temporal");
        builder.HasKey(x => x.TemporaryClientOperationId);
        builder.Property(x => x.TemporaryClientOperationId).HasColumnName("id");
        builder.Property(x => x.RegistrationDate).HasColumnName("fecha_radicacion");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.ObjectiveId).HasColumnName("objetivo_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.Amount).HasColumnName("valor");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.OperationTypeId).HasColumnName("tipo_operaciones_id");
        builder.Property(x => x.ApplicationDate).HasColumnName("fecha_aplicacion");
        builder.Property(x => x.Processed).HasColumnName("procesado");
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasColumnName("estado");
        builder.Property(x => x.TrustId).HasColumnName("fideicomiso_id");
        builder.Property(x => x.LinkedClientOperationId).HasColumnName("operaciones_cliente_id");
        builder.Property(x => x.CauseId).HasColumnName("causal_id");
        builder.Property(x => x.Units)
            .HasColumnName("unidades")
            .HasColumnType("decimal(38, 16)");

        builder.Ignore(x => x.OperationType);
    }
}
