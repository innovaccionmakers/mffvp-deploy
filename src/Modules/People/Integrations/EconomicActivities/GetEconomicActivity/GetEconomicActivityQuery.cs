using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.EconomicActivities.GetEconomicActivity;

public sealed record GetEconomicActivityQuery(
    string EconomicActivityId
) : IQuery<EconomicActivityResponse>;