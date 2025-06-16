using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Integrations.People.CreatePerson
{
    public sealed record CreatePersonRequestCommand(
        [property: JsonPropertyName("CodigoHomologado")]
        string? HomologatedCode,

        [property: JsonPropertyName("TipoIdentificacion")]
        [property: HomologScope("TipoDocumento")]
        string DocumentType,

        [property: JsonPropertyName("Identificacion")]
        string Identification,

        [property: JsonPropertyName("PrimerNombre")]
        string FirstName,

        [property: JsonPropertyName("SegundoNombre")]
        string? MiddleName,

        [property: JsonPropertyName("PrimerApellido")]
        string LastName,

        [property: JsonPropertyName("SegundoApellido")]
        string? SecondLastName,

        [property: JsonPropertyName("FechaNacimiento")]
        DateTime BirthDate,

        [property: JsonPropertyName("Celular")]
        string Mobile,

        [property: JsonPropertyName("Sexo")]
        [property: HomologScope("Sexo")]
        string Gender,

        [property: JsonPropertyName("PaisResidencia")]
        string CountryOfResidence,

        [property: JsonPropertyName("Departamento")]
        string Department,

        [property: JsonPropertyName("Municipio")]
        string Municipality,

        [property: JsonPropertyName("Email")]
        string Email,

        [property: JsonPropertyName("ActividadEconomica")]
        string EconomicActivity,

        [property: JsonPropertyName("Direccion")]
        string Address,

        [property: JsonPropertyName("Declaracion")]
        bool Declarant,

        [property: JsonPropertyName("TipoInversionista")]
        [property: HomologScope("TipoInversionista")]
        string InvestorType,

        [property: JsonPropertyName("PerfilRiesgo")]
        [property: HomologScope("PerfilRiesgo")]
        string RiskProfile
    ) : ICommand;
}