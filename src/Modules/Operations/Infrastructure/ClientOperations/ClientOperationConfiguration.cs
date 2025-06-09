using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;

namespace Operations.Infrastructure.ClientOperations;

internal sealed class ClientOperationConfiguration : IEntityTypeConfiguration<ClientOperation>
{
    public void Configure(EntityTypeBuilder<ClientOperation> builder)
    {
        builder.ToTable("operaciones_clientes");
        builder.HasKey(x => x.ClientOperationId);
        builder.Property(x => x.ClientOperationId).HasColumnName("id");
        builder.Property(x => x.RegistrationDate).HasColumnName("fecha_radicacion");
        builder.Property(x => x.AffiliateId).HasColumnName("afiliado_id");
        builder.Property(x => x.ObjectiveId).HasColumnName("objetivo_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.Amount).HasColumnName("valor");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.SubtransactionTypeId).HasColumnName("subtipo_transaccion_id");

        builder.HasOne(x => x.SubtransactionType)
            .WithMany(st => st.ClientOperations)
            .HasForeignKey(x => x.SubtransactionTypeId);

        builder.HasOne(x => x.AuxiliaryInformation)
            .WithOne(ai => ai.ClientOperation)
            .HasForeignKey<AuxiliaryInformation>(ai => ai.ClientOperationId);
    }
}