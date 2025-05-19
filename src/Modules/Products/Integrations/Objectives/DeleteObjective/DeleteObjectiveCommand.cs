using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.DeleteObjective;

public sealed record DeleteObjectiveCommand(
    long ObjectiveId
) : ICommand;