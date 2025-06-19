using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Integrations.PensionRequirements.CreatePensionRequirement;
public sealed record CreatePensionRequirementCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
    string DocumentType,
    
    [property: JsonPropertyName("Identificacion")]
    [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
    string Identification,
    
    [property: JsonPropertyName("FechaInicioReqPen")]
    [property: JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
    DateTime? StartDateReqPen,
    
    [property: JsonPropertyName("FechaFinReqPen")]
    [property: JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
    DateTime? EndDateReqPen
) : ICommand;