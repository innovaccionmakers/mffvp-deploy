using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Products.Integrations.Plans.GetPlans;
public sealed record GetPlansQuery() : IQuery<IReadOnlyCollection<PlanResponse>>;