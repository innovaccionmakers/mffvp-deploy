using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.GetObjective;

public sealed record GetObjectiveQuery(
    long ObjectiveId
) : IQuery<ObjectiveResponse>;