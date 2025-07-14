using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasury.Domain.TreasuryConcepts;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;

namespace Treasury.Infrastructure.TreasuryConcepts;

internal sealed class TreasuryConceptConfiguration : IEntityTypeConfiguration<TreasuryConcept>
{
    public void Configure(EntityTypeBuilder<TreasuryConcept> builder)
    {
        builder.ToTable("conceptos_tesoreria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Concept).HasColumnName("concepto");
        builder.Property(x => x.Nature)
            .HasColumnName("naturaleza")
            .HasConversion(new EnumMemberValueConverter<IncomeExpenseNature>());
        builder.Property(x => x.AllowsNegative).HasColumnName("admite_negativo");
        builder.Property(x => x.AllowsExpense).HasColumnName("permite_gasto");
        builder.Property(x => x.RequiresBankAccount).HasColumnName("cuenta_bancaria");
        builder.Property(x => x.RequiresCounterparty).HasColumnName("contraparte");
        builder.Property(x => x.ProcessDate).HasColumnName("fecha_proceso");
        builder.Property(x => x.Observations).HasColumnName("observaciones");
    }
}