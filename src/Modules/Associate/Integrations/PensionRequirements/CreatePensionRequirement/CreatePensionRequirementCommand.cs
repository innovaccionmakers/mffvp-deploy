using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.PensionRequirements.CreatePensionRequirement;
public sealed record CreatePensionRequirementCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    string DocumentType,
    
    [property: JsonPropertyName("Identificacion")]
    string Identification,
    
    [property: JsonPropertyName("FechaInicioReqPen")]
    DateTime StartDateReqPen,
    
    [property: JsonPropertyName("FechaFinReqPen")]
    DateTime EndDateReqPen
) : ICommand;