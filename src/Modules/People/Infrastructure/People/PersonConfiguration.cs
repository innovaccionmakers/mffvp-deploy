
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using People.Domain.People;
using People.Domain.Countries;
using People.Domain.EconomicActivities;

namespace People.Infrastructure.People;
internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("personas");  
        builder.HasKey(x => x.PersonId);
        builder.Property(x => x.PersonId).HasColumnName("persona_id");
        builder.Property(x => x.DocumentType).HasColumnName("tipo_documento");
        builder.Property(x => x.StandardCode).HasColumnName("codigo_homologado");
        builder.Property(x => x.Identification).HasColumnName("identificacion");
        builder.Property(x => x.FirstName).HasColumnName("primer_nombre");
        builder.Property(x => x.MiddleName).HasColumnName("segundo_nombre");
        builder.Property(x => x.LastName).HasColumnName("primer_apellido");
        builder.Property(x => x.SecondLastName).HasColumnName("segundo_apellido");
        builder.Property(x => x.IssueDate).HasColumnName("fecha_expedicion");
        builder.Property(x => x.IssueCityId).HasColumnName("ciudad_expedicion_id");
        builder.Property(x => x.BirthDate).HasColumnName("fecha_nacimiento");
        builder.Property(x => x.BirthCityId).HasColumnName("ciudad_nacimiento_id");
        builder.Property(x => x.Mobile).HasColumnName("celular");
        builder.Property(x => x.FullName).HasColumnName("nombre_completo");
        builder.Property(x => x.MaritalStatusId).HasColumnName("estado_civil_id");
        builder.Property(x => x.GenderId).HasColumnName("sexo_id");
        builder.Property(x => x.Email).HasColumnName("email");
        builder.HasOne<Country>()
               .WithMany()
               .HasForeignKey(x => x.CountryId);
        builder.HasOne<EconomicActivity>()
               .WithMany()
               .HasForeignKey(x => x.EconomicActivityId);
    }
}