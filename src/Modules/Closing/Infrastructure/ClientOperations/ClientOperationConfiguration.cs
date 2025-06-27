using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Closing.Domain.ClientOperations;

namespace Closing.Infrastructure.ClientOperations;

internal sealed class ClientOperationConfiguration : IEntityTypeConfiguration<ClientOperation>
{
    public void Configure(EntityTypeBuilder<ClientOperation> builder)
    {
        builder.ToTable("operaciones_cliente");
        builder.HasKey(x => x.ClientOperationId);
        builder.Property(x => x.ClientOperationId).HasColumnName("id");
        builder.Property(x => x.FilingDate).HasColumnName("fecha_radicacion");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.ObjectiveId).HasColumnName("objetivo_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.Amount).HasColumnName("valor").HasColumnType("decimal(19, 2)");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.TransactionSubtypeId).HasColumnName("subtipo_transaccion_id");
        builder.Property(x => x.ApplicationDate).HasColumnName("fecha_aplicacion");
    }
}