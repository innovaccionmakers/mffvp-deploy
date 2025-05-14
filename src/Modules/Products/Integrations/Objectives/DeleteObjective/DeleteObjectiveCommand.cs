using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Objectives.DeleteObjective;
public sealed record DeleteObjectiveCommand(
    long ObjectiveId
) : ICommand;