using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Objectives.CreateObjective;
public sealed record CreateObjectiveCommand(
    int ObjectiveTypeId,
    int AffiliateId,
    int AlternativeId,
    string Name,
    string Status,
    DateTime CreationDate
) : ICommand<ObjectiveResponse>;