using Common.SharedKernel.Domain;
using Common.SharedKernel.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Customers.Domain.People;

namespace Customers.Infrastructure.People;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("personas");

        builder.HasKey(x => x.PersonId);
        builder.Property(x => x.PersonId).HasColumnName("id");
        builder.Property(x => x.DocumentType).HasColumnName("tipo_documento_uuid");
        builder.Property(x => x.Identification).HasColumnName("identificacion");
        builder.Property(x => x.FirstName).HasColumnName("primer_nombre");
        builder.Property(x => x.MiddleName).HasColumnName("segundo_nombre");
        builder.Property(x => x.LastName).HasColumnName("primer_apellido");
        builder.Property(x => x.SecondLastName).HasColumnName("segundo_apellido");
        builder.Property(x => x.BirthDate).HasColumnName("fecha_nacimiento");
        builder.Property(x => x.FullName).HasColumnName("nombre_completo");
        builder.Property(x => x.Mobile).HasColumnName("celular");
        builder.Property(x => x.GenderId).HasColumnName("sexo_id");
        builder.Property(x => x.CountryOfResidenceId).HasColumnName("pais_residencia_id");
        builder.Property(x => x.DepartmentId).HasColumnName("departamento_id");
        builder.Property(x => x.MunicipalityId).HasColumnName("municipio_id");
        builder.Property(x => x.Email).HasColumnName("email");
        builder.Property(x => x.EconomicActivityId).HasColumnName("actividad_economica_id");
        builder.Property(x => x.Address).HasColumnName("direccion");
        builder.Property(x => x.IsDeclarant).HasColumnName("declarante");
        builder.Property(x => x.InvestorTypeId).HasColumnName("tipo_inversionista_id");
        builder.Property(x => x.RiskProfileId).HasColumnName("perfil_riesgo_id");
        builder.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion(new EnumMemberValueConverter<Status>());
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
    }
}