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
        builder.Property(x => x.SubtransactionTypeId).HasColumnName("subtipo_transaccion_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.RegistrationDate).HasColumnName("fecha_radicacion");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.ApplicationDate).HasColumnName("fecha_aplicacion");

        builder.HasOne(x => x.ClientOperation)
            .WithMany(co => co.TrustOperations)
            .HasForeignKey(x => x.ClientOperationId)
            .IsRequired(false);
    }
}