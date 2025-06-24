using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Integrations.People.CreatePerson
{
    public sealed record CreatePersonRequestCommand(
        [property: JsonPropertyName("CodigoHomologado")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string? HomologatedCode,

        [property: JsonPropertyName("TipoIdentificacion")]
        [property: HomologScope("TipoDocumento")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string DocumentType,

        [property: JsonPropertyName("Identificacion")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string Identification,

        [property: JsonPropertyName("PrimerNombre")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string FirstName,

        [property: JsonPropertyName("SegundoNombre")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string? MiddleName,

        [property: JsonPropertyName("PrimerApellido")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string LastName,

        [property: JsonPropertyName("SegundoApellido")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string? SecondLastName,

        [property: JsonPropertyName("FechaNacimiento")]
        [property: JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
        DateTime? BirthDate,

        [property: JsonPropertyName("Celular")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string Mobile,

        [property: JsonPropertyName("Sexo")]
        [property: HomologScope("Sexo")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string Gender,

        [property: JsonPropertyName("Direccion")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string Address,

        [property: JsonPropertyName("Departamento")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string Department,

        [property: JsonPropertyName("Municipio")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string Municipality,

        [property: JsonPropertyName("PaisResidencia")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string CountryOfResidence,

        [property: JsonPropertyName("Email")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string Email,

        [property: JsonPropertyName("ActividadEconomica")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string EconomicActivity,

        [property: JsonPropertyName("Declarante")]
        [property: JsonConverter(typeof(BooleanOrStringToBooleanConverter))]
        bool? Declarant,

        [property: JsonPropertyName("PerfilRiesgo")]
        [property: HomologScope("PerfilRiesgo")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string RiskProfile,

        [property: JsonPropertyName("TipoInversionista")]
        [property: HomologScope("TipoInversionista")]
        [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
        string InvestorType
    ) : ICommand;
}