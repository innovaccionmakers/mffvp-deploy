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
        builder.Property(x => x.SubtransactionTypeId).HasColumnName("subtipo_transaccion_id");
        builder.Property(x => x.ApplicationDate).HasColumnName("fecha_aplicacion");

        builder.Ignore(x => x.SubtransactionType);
    }
}
