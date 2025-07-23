using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Products.Integrations.Objectives.CreateObjective;
using System.Text.Json.Serialization;

namespace Products.Integrations.Objectives.UpdateObjective;

public sealed record UpdateObjectiveCommand(
    [property: JsonPropertyName("IdObjetivo")]
    int ObjectiveId,

    [property: JsonPropertyName("TipoObjetivo")]
    [property: HomologScope("TipoObjetivo")]
    string ObjectiveType,

    [property: JsonPropertyName("NombreObjetivo")]
    string ObjectiveName,

    [property: JsonPropertyName("OficinaApertura")]
    string OpeningOffice,

    [property: JsonPropertyName("OficinaActual")]
    string CurrentOffice,

    [property: JsonPropertyName("Comercial")]
    string Commercial,

    [property: JsonPropertyName("Estado")]
    string Status
) : ICommand;