using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Customers.Integrations.EconomicActivities.GetEconomicActivitys;
public sealed record GetEconomicActivitysQuery() : IQuery<IReadOnlyCollection<EconomicActivityResponse>>;