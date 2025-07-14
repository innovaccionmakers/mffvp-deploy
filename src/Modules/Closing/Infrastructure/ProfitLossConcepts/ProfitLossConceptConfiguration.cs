using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Closing.Domain.ProfitLossConcepts;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;

namespace Closing.Infrastructure.ProfitLossConcepts;

internal sealed class ProfitLossConceptConfiguration : IEntityTypeConfiguration<ProfitLossConcept>
{
    public void Configure(EntityTypeBuilder<ProfitLossConcept> builder)
    {
        builder.ToTable("conceptos_pyg");
        builder.HasKey(x => x.ProfitLossConceptId);
        builder.Property(x => x.ProfitLossConceptId).HasColumnName("id");
        builder.Property(x => x.Concept).HasColumnName("concepto");
        builder.Property(x => x.Nature)
            .HasColumnName("naturaleza")
            .HasConversion(new EnumMemberValueConverter<IncomeExpenseNature>());
        builder.Property(x => x.AllowNegative).HasColumnName("admite_negativo");
    }
}