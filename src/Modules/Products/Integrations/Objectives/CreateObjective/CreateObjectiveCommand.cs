using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.CreateObjective;

public sealed record CreateObjectiveCommand(
    [property: JsonPropertyName("TipoId")]
    string IdType,
    
    [property: JsonPropertyName("Identificacion")]
    string Identification,
    
    [property: JsonPropertyName("IdAlternativa")]
    [property: HomologScope("Tipos Alternativas")]
    string AlternativeId,
    
    [property: JsonPropertyName("TipoObjetivo")]
    [property: HomologScope("Tipos Objetivos")]
    string ObjectiveType,
    
    [property: JsonPropertyName("NombreObjetivo")]
    string ObjectiveName,
    
    [property: JsonPropertyName("OficinaApertura")]
    string OpeningOffice,
    
    [property: JsonPropertyName("OficinaActual")]
    string CurrentOffice,
    
    [property: JsonPropertyName("Comercial")]
    string Commercial
) : ICommand<ObjectiveResponse>;