using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operations.Domain.AuxiliaryInformations;

namespace Operations.Infrastructure.AuxiliaryInformations;

internal sealed class AuxiliaryInformationConfiguration : IEntityTypeConfiguration<AuxiliaryInformation>
{
    public void Configure(EntityTypeBuilder<AuxiliaryInformation> builder)
    {
        builder.ToTable("informacion_auxiliar");
        builder.HasKey(x => x.AuxiliaryInformationId);
        builder.Property(x => x.AuxiliaryInformationId).HasColumnName("id");
        builder.Property(x => x.ClientOperationId).HasColumnName("operacion_cliente_id");
        builder.Property(x => x.OriginId).HasColumnName("origen_id");
        builder.Property(x => x.CollectionMethodId).HasColumnName("metodo_recaudo_id");
        builder.Property(x => x.PaymentMethodId).HasColumnName("forma_pago_id");
        builder.Property(x => x.CollectionAccount).HasColumnName("cuenta_recaudo");
        builder.Property(x => x.PaymentMethodDetail).HasColumnName("detalle_forma_pago");
        builder.Property(x => x.CertificationStatusId).HasColumnName("estado_certificacion_id");
        builder.Property(x => x.TaxConditionId).HasColumnName("condicion_tributaria_id");
        builder.Property(x => x.ContingentWithholding).HasColumnName("retencion_contingente");
        builder.Property(x => x.VerifiableMedium).HasColumnName("medio_verificable");
        builder.Property(x => x.CollectionBankId).HasColumnName("banco_recaudo");
        builder.Property(x => x.DepositDate).HasColumnName("fecha_consignacion");
        builder.Property(x => x.SalesUser).HasColumnName("usuario_comercial");
        builder.Property(x => x.OriginModalityId).HasColumnName("modalidad_origen_id");
        builder.Property(x => x.CityId).HasColumnName("ciudad_id");
        builder.Property(x => x.ChannelId).HasColumnName("canal_id");
        builder.Property(x => x.UserId).HasColumnName("usuario_id");

        builder.HasOne(x => x.ClientOperation)
            .WithOne(co => co.AuxiliaryInformation)
            .HasForeignKey<AuxiliaryInformation>(x => x.ClientOperationId);

        builder.HasOne(x => x.Origin)
            .WithMany(o => o.AuxiliaryInformations)
            .HasForeignKey(x => x.OriginId);

        builder.HasOne(x => x.Bank)
            .WithMany(b => b.AuxiliaryInformations)
            .HasForeignKey(x => x.CollectionBankId)
            .HasPrincipalKey(b => b.BankId);
    }
}