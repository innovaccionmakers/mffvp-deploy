using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.ClientOperations;

namespace Operations.Infrastructure.ClientOperations;

internal sealed class ClientOperationConfiguration : IEntityTypeConfiguration<ClientOperation>
{
    public void Configure(EntityTypeBuilder<ClientOperation> builder)
    {
        builder.ToTable("operaciones_clientes");
        builder.HasKey(x => x.ClientOperationId);
        builder.Property(x => x.ClientOperationId).HasColumnName("id");
        builder.Property(x => x.Date).HasColumnName("fecha");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.ObjectiveId).HasColumnName("objetivo_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.Amount).HasColumnName("valor");
        builder.Property(x => x.SubtransactionTypeId).HasColumnName("subtipotransaccion_id");
    }
}