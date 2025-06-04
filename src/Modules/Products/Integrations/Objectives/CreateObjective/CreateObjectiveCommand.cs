using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.CreateObjective;

public sealed record CreateObjectiveCommand(
    string IdType,
    string Identification,
    [property: HomologScope("Tipos Alternativas")]
    string AlternativeId,
    [property: HomologScope("Tipos Objetivos")]
    string ObjectiveType,
    string ObjectiveName,
    string OpeningOffice,
    string CurrentOffice,
    string Commercial
) : ICommand<ObjectiveResponse>;