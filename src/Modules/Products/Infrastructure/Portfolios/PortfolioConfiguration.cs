using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Portfolios;

namespace Products.Infrastructure.Portfolios;

internal sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portafolios");
        builder.HasKey(x => x.PortfolioId);
        builder.Property(x => x.PortfolioId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.ShortName).HasColumnName("nombre_corto");
        builder.Property(x => x.ModalityId).HasColumnName("modalidad_id");
        builder.Property(x => x.InitialMinimumAmount).HasColumnName("aporte_minimo_inicial");
        builder.Property(x => x.AdditionalMinimumAmount).HasColumnName("aporte_minimo_adicional");
        builder.Property(x => x.CurrentDate).HasColumnName("fecha_actual");
        builder.Property(x => x.CommissionRateTypeId).HasColumnName("tipo_tasa_comision");
        builder.Property(x => x.CommissionPercentage).HasColumnName("porcentaje_comision");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologacion");
        builder.HasMany(p => p.Alternatives)
    .WithOne(ap => ap.Portfolio)
    .HasForeignKey(ap => ap.PortfolioId);

        builder.HasMany(p => p.Commissions)
    .WithOne(c => c.Portfolio)
    .HasForeignKey(c => c.PortfolioId);

        builder.HasMany(p => p.PortfolioValuations)
            .WithOne(pv => pv.Portfolio)
            .HasForeignKey(pv => pv.PortfolioId);
        builder.Property(x => x.VerificationDigit).HasColumnName("digito_verificacion");
        builder.Property(x => x.PortfolioNIT).HasColumnName("nit_portafolio");
        builder.Property(x => x.NitApprovedPortfolio).HasColumnName("nit_portafolio_homologado");
        builder.Property(x => x.RiskProfile).HasColumnName("perfil_riesgo");
        builder.Property(x => x.SFCBusinessCode).HasColumnName("cod_Negocio_SFC");
        builder.Property(x => x.Custodian).HasColumnName("custodio");
        builder.Property(x => x.Qualifier).HasColumnName("calificadora");
        builder.Property(x => x.Rating).HasColumnName("calificacion");
        builder.Property(x => x.RatingType).HasColumnName("tipo_calificacion");
        builder.Property(x => x.LastRatingDate).HasColumnName("fecha_ultima_calificacion");
        builder.Property(x => x.AdviceClassification).HasColumnName("clasificacion_asesoria");
        builder.Property(x => x.MaxParticipationPercentage).HasColumnName("porc_max_participacion");
        builder.Property(x => x.MinimumVirPercentage).HasColumnName("vir_retiro_minimo");
        builder.Property(x => x.PartialVirPercentage).HasColumnName("vir_retiro_max_parcial");
        builder.Property(x => x.AgileWithdrawalPercentageProtectedBalance).HasColumnName("porc_retiro_agil_saldo_protegido");
        builder.Property(x => x.WithdrawalPercentageProtectedBalance).HasColumnName("porc_retiro_saldo_protegido");
        builder.Property(x => x.AllowsAgileWithdrawal).HasColumnName("permite_retiro_agil");
        builder.Property(x => x.PermanencePeriod).HasColumnName("plazo_permanencia");
        builder.Property(x => x.PenaltyPercentage).HasColumnName("porcentaje_penalizacion");
        builder.Property(x => x.OperationsStartDate).HasColumnName("fecha_inicio_operaciones");
        builder.Property(x => x.PortfolioExpiryDate).HasColumnName("fecha_vto_portafolio");
        builder.Property(x => x.IndustryClassification).HasColumnName("clasificacion_industria");
    }
}