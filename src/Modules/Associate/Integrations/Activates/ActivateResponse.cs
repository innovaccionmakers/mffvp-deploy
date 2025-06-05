using System.Text.Json.Serialization;

namespace Associate.Integrations.Activates;

public sealed record ActivateResponse(
    [property: JsonPropertyName("AfiliadoId")]
    int ActivateId,

    [property: JsonPropertyName("TipoId")]
    string IdentificationType,

    [property: JsonPropertyName("Identificacion")]
    string Identification,

    [property: JsonPropertyName("Pensionado")]
    bool Pensioner,

    [property: JsonPropertyName("CumpleRequisitosPension")]
    bool MeetsPensionRequirements,
    
    [property: JsonPropertyName("FechaActivacion")]
    DateTime ActivateDate
);