using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.GetObjectives;

public sealed record GetObjectivesQuery : IQuery<IReadOnlyCollection<ObjectiveResponse>>;