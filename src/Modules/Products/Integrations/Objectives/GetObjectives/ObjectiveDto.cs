using System.Text.Json.Serialization;
using Common.SharedKernel.Domain;

namespace Products.Integrations.Objectives.GetObjectives;

public sealed record ObjectiveDto(
    [property: JsonPropertyName("IdObjetivo")]
    int ObjectiveId,

    [property: JsonPropertyName("TipoObjetivo")]
    string ObjectiveType,

    [property: JsonPropertyName("NombreObjetivo")]
    string ObjectiveName,

    [property: JsonPropertyName("Fondo")]
    string FundName,

    [property: JsonPropertyName("Plan")]
    string PlanName,

    [property: JsonPropertyName("IdAlternativa")]
    string AlternativeId,

    [property: JsonPropertyName("Alternativa")]
    string AlternativeName,

    [property: JsonPropertyName("PortafolioRecaudador")]
    string CollectorPortfolioName,

    [property: JsonPropertyName("Estado")]
    string Status
);