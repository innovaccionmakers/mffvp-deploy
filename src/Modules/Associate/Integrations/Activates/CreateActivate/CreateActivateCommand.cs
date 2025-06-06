using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.CreateActivate;

public sealed record CreateActivateCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    string IdentificationType,

    [property: JsonPropertyName("Identificacion")]
    string Identification,

    [property: JsonPropertyName("Pensionado")]
    bool Pensioner,

    [property: JsonPropertyName("CumpleRequisitosPension")]
    bool? MeetsPensionRequirements,

    [property: JsonPropertyName("FechaInicioReqPen")]
    DateTime? StartDateReqPen,

    [property: JsonPropertyName("FechaFinReqPen")]
    DateTime? EndDateReqPen
) : ICommand;