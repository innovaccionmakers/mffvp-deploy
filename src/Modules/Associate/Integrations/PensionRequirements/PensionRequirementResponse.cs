using Common.SharedKernel.Core.Primitives;

using System.Text.Json.Serialization;

namespace Associate.Integrations.PensionRequirements;

public sealed record PensionRequirementResponse(
    [property: JsonPropertyName("IdRequisitoPension")]
    int PensionRequirementId,

    [property: JsonPropertyName("AfiliadoId")]
    int ActivateId,
    
    [property: JsonPropertyName("FechaInicio")]
    DateTime StartDate,
    
    [property: JsonPropertyName("FechaVencimiento")]
    DateTime ExpirationDate,
    
    [property: JsonPropertyName("FechaCreacion")]
    DateTime CreationDate,
    
    [property: JsonPropertyName("Estado")]
    Status Status
);