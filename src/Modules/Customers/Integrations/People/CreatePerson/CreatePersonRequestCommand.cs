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
        [property: HomologScope("CodigosDeError")]
        string Gender,

        [property: JsonPropertyName("PaisResidencia")]
        [property: HomologScope("CodigosDeError")]
        string CountryOfResidence,

        [property: JsonPropertyName("Departamento")]
        [property: HomologScope("CodigosDeError")]
        string Department,

        [property: JsonPropertyName("Municipio")]
        [property: HomologScope("CodigosDeError")]
        string Municipality,

        [property: JsonPropertyName("Email")]
        string Email,

        [property: JsonPropertyName("ActividadEconomica")]
        [property: HomologScope("CodigosDeError")]
        string EconomicActivity,

        [property: JsonPropertyName("Direccion")]
        string Address,

        [property: JsonPropertyName("Declarante")]
        [property: HomologScope("CodigosDeError")]
        bool Declarant,

        [property: JsonPropertyName("TipoInversionista")]
        [property: HomologScope("CodigosDeError")]
        string InvestorType,

        [property: JsonPropertyName("PerfilRiesgo")]
        [property: HomologScope("CodigosDeError")]
        string RiskProfile
    ) : ICommand;
}