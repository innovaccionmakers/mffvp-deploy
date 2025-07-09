using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasury.Domain.TreasuryMovements;

namespace Treasury.Infrastructure.TreasuryMovements;

internal sealed class TreasuryMovementConfiguration : IEntityTypeConfiguration<TreasuryMovement>
{
    public void Configure(EntityTypeBuilder<TreasuryMovement> builder)
    {
        builder.ToTable("movimientos_tesoreria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PortfolioId).HasColumnName("portafolio_id");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.ClosingDate).HasColumnName("fecha_cierre");
        builder.Property(x => x.TreasuryConceptId).HasColumnName("concepto_tesoreria_id");
        builder.Property(x => x.Value).HasColumnName("valor").HasColumnType("decimal(19,2)");
        builder.Property(x => x.BankAccountId).HasColumnName("cuenta_bancaria_id");
        builder.Property(x => x.EntityId).HasColumnName("entidad_id");
        builder.Property(x => x.CounterpartyId).HasColumnName("contraparte_id");

        builder.HasOne(x => x.TreasuryConcept)
            .WithMany(x => x.TreasuryMovements)
            .HasForeignKey(x => x.TreasuryConceptId);

        builder.HasOne(x => x.BankAccount)
            .WithMany(x => x.TreasuryMovements)
            .HasForeignKey(x => x.BankAccountId);

        builder.HasOne(x => x.Entity)
            .WithMany(x => x.TreasuryMovementsAsEntity)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Counterparty)
            .WithMany(x => x.TreasuryMovementsAsCounterparty)
            .HasForeignKey(x => x.CounterpartyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}