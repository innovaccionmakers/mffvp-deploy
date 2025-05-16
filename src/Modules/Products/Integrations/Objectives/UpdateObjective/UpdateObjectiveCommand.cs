using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Objectives.UpdateObjective;
public sealed record UpdateObjectiveCommand(
    long ObjectiveId,
    int NewObjectiveTypeId,
    int NewAffiliateId,
    int NewAlternativeId,
    string NewName,
    string NewStatus,
    DateTime NewCreationDate
) : ICommand<ObjectiveResponse>;