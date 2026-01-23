using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Infrastructure.Database;
using DomainProductSheet = Reports.Domain.LoadingInfo.Products.ProductSheet;

namespace Reports.Infrastructure.LoadingInfo.Products;

public sealed class ProductSheetConfiguration : IEntityTypeConfiguration<DomainProductSheet>
{
    public void Configure(EntityTypeBuilder<DomainProductSheet> builder)
    {
        builder.ToTable("sabana_productos", Schemas.Reports);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
          .HasColumnName("id")
          .ValueGeneratedOnAdd()
          .IsRequired();

        builder.Property(x => x.AdministratorId)
         .HasColumnName("administrador_id")
        .IsRequired();

        builder.Property(x => x.EntityType)
      .HasColumnName("tipo_entidad")
        .IsRequired();

        builder.Property(x => x.EntityCode)
        .HasColumnName("codigo_entidad")
       .HasColumnType("text")
       .IsRequired();

        builder.Property(x => x.BusinessCodeSfcFund)
        .HasColumnName("cod_negocio_sfc_fondo")
        .IsRequired();

        builder.Property(x => x.FundId)
       .HasColumnName("fondo_id")
       .IsRequired();

        builder.Property(x => x.EntitySfcCode)
       .HasColumnName("codigo_entidad_sfc")
       .HasMaxLength(6)
        .HasColumnType("character varying(6)")
        .IsRequired();
    }
}