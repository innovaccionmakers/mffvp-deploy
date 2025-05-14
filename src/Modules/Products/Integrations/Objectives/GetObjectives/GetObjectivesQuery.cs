using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Products.Integrations.Objectives.GetObjectives;
public sealed record GetObjectivesQuery() : IQuery<IReadOnlyCollection<ObjectiveResponse>>;