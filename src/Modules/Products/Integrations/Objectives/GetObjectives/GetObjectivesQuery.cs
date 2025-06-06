using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.GetObjectives;

public record GetObjectivesQuery(
    [property: JsonPropertyName("TipoId")]
    string TypeId,
    
    [property: JsonPropertyName("Identificacion")]
    string Identification,
    
    [property: JsonPropertyName("Estado")]
    StatusType Status
) : IQuery<GetObjectivesResponse>;