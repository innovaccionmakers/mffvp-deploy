using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace People.Integrations.EconomicActivities.GetEconomicActivities;
public sealed record GetEconomicActivitiesQuery() : IQuery<IReadOnlyCollection<EconomicActivityResponse>>;