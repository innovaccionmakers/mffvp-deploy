using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.OperationTypes;

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
        builder.Property(x => x.OperationTypeId).HasColumnName("tipo_operaciones_id");
        builder.Property(x => x.ApplicationDate).HasColumnName("fecha_aplicacion");

        builder.HasOne(x => x.OperationType)
            .WithMany(st => st.ClientOperations)
            .HasForeignKey(x => x.OperationTypeId);

        builder.HasOne(x => x.AuxiliaryInformation)
            .WithOne(ai => ai.ClientOperation)
            .HasForeignKey<AuxiliaryInformation>(ai => ai.ClientOperationId);

        builder.HasMany(x => x.TrustOperations)
            .WithOne(to => to.ClientOperation)
            .HasForeignKey(to => to.ClientOperationId);
    }
}