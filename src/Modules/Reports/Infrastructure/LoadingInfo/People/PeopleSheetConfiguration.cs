using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Infrastructure.Database;
using DomainPeopleSheet = Reports.Domain.LoadingInfo.People.PeopleSheet;

namespace Reports.Infrastructure.LoadingInfo.People;

internal sealed class PeopleSheetConfiguration : IEntityTypeConfiguration<DomainPeopleSheet>
{
    public void Configure(EntityTypeBuilder<DomainPeopleSheet> builder)
    {
        builder.ToTable("sabana_personas_afiliados", Schemas.Reports);

        builder.HasKey(x => x.Id)
               .HasName("sabana_personas_afiliado_pkey");

        builder.Property(x => x.Id)
          .HasColumnName("id")
         .ValueGeneratedOnAdd();

        builder.Property(x => x.IdentificationType)
         .HasColumnName("tipo_identificacion")
          .IsRequired();

        builder.Property(x => x.NormalizedIdentificationType)
            .HasColumnName("tipo_identificacion_homologado")
             .IsRequired();

        builder.Property(x => x.AffiliateId)
            .HasColumnName("afiliado_id")
            .IsRequired();

        builder.Property(x => x.IdentificationNumber)
            .HasColumnName("identificacion")
             .IsRequired();

        builder.Property(x => x.FullName)
            .HasColumnName("nombre_completo")
             .IsRequired();

        builder.Property(x => x.BirthDate)
            .HasColumnName("fecha_nacimiento")
            .HasColumnType("timestamp without time zone");

        builder.Property(x => x.Gender)
        .HasColumnName("sexo")
        .IsRequired();
    }
}