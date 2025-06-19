using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Integrations.Activates.CreateActivate;

public sealed record CreateActivateCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    string DocumentType,

    [property: JsonPropertyName("Identificacion")]
    string Identification,

    [property: JsonPropertyName("Pensionado")]
    [property: JsonConverter(typeof(BooleanOrStringToBooleanConverter))]
    bool? Pensioner,

    [property: JsonPropertyName("CumpleRequisitosPension")]    
    [property: JsonConverter(typeof(BooleanOrStringToBooleanConverter))]
    bool? MeetsPensionRequirements,

    [property: JsonPropertyName("FechaInicioReqPen")]
    [property: JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
    DateTime? StartDateReqPen,

    [property: JsonPropertyName("FechaFinReqPen")]
    [property: JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
    DateTime? EndDateReqPen
) : ICommand;