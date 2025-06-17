using System.Text.Json.Serialization;
using Common.SharedKernel.Domain;

namespace Associate.Integrations.Activates;

public sealed record ActivateResponse(
    [property: JsonPropertyName("AfiliadoId")]
    int ActivateId,

    [property: JsonPropertyName("TipoId")]
    string DocumentType,

    [property: JsonPropertyName("Identificacion")]
    string Identification,

    [property: JsonPropertyName("Pensionado")]
    bool Pensioner,

    [property: JsonPropertyName("CumpleRequisitosPension")]
    bool MeetsPensionRequirements,
    
    [property: JsonPropertyName("FechaActivacion")]
    DateTime ActivateDate
);