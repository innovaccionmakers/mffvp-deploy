using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Objectives.GetObjective;
public sealed record GetObjectiveQuery(
    long ObjectiveId
) : IQuery<ObjectiveResponse>;