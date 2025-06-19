using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Integrations.Activates.UpdateActivate;

public sealed record UpdateActivateCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
    string DocumentType,

    [property: JsonPropertyName("Identificacion")]
    [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
    string Identification,

    [property: JsonPropertyName("Pensionado")]    
    [property: JsonConverter(typeof(BooleanOrStringToBooleanConverter))]
    bool? Pensioner
) : ICommand;