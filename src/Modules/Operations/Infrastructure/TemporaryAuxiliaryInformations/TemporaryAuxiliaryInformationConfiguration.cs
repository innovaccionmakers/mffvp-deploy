using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.TemporaryAuxiliaryInformations;

namespace Operations.Infrastructure.TemporaryAuxiliaryInformations;

internal sealed class TemporaryAuxiliaryInformationConfiguration : IEntityTypeConfiguration<TemporaryAuxiliaryInformation>
{
    public void Configure(EntityTypeBuilder<TemporaryAuxiliaryInformation> builder)
    {
        builder.ToTable("informacion_auxiliar_temporal");
        builder.HasKey(x => x.TemporaryAuxiliaryInformationId);
        builder.Property(x => x.TemporaryAuxiliaryInformationId).HasColumnName("id");
        builder.Property(x => x.TemporaryClientOperationId).HasColumnName("operacion_cliente_temporal_id");
        builder.Property(x => x.OriginId).HasColumnName("origen_id");
        builder.Property(x => x.CollectionMethodId).HasColumnName("metodo_recaudo_id");
        builder.Property(x => x.PaymentMethodId).HasColumnName("forma_pago_id");
        builder.Property(x => x.CollectionAccount).HasColumnName("cuenta_recaudo");
        builder.Property(x => x.PaymentMethodDetail).HasColumnName("detalle_forma_pago");
        builder.Property(x => x.CertificationStatusId).HasColumnName("estado_certificacion_id");
        builder.Property(x => x.TaxConditionId).HasColumnName("condicion_tributaria_id");
        builder.Property(x => x.ContingentWithholding)
            .HasColumnName("retencion_contingente")
            .HasColumnType("decimal(19, 2)");
        builder.Property(x => x.VerifiableMedium).HasColumnName("medio_verificable");
        builder.Property(x => x.CollectionBankId).HasColumnName("banco_recaudo");
        builder.Property(x => x.DepositDate).HasColumnName("fecha_consignacion");
        builder.Property(x => x.SalesUser).HasColumnName("usuario_comercial");
        builder.Property(x => x.OriginModalityId).HasColumnName("modalidad_origen_id");
        builder.Property(x => x.CityId).HasColumnName("ciudad_id");
        builder.Property(x => x.ChannelId).HasColumnName("canal_id");
        builder.Property(x => x.UserId).HasColumnName("usuario_id")
            .HasMaxLength(50);
        
        builder.Ignore(x => x.Channel);
        builder.Ignore(x => x.Origin);
    
        builder.HasOne(x => x.TemporaryClientOperation)
            .WithOne(co => co.TemporaryAuxiliaryInformation)
            .HasForeignKey<TemporaryAuxiliaryInformation>(x => x.TemporaryClientOperationId);
    }
}
