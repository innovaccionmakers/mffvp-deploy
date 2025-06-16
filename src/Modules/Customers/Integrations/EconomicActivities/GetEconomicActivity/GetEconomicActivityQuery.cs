using Common.SharedKernel.Application.Messaging;
using System;

namespace Customers.Integrations.EconomicActivities.GetEconomicActivity;

public sealed record GetEconomicActivityQuery(
    string HomologatedCode
) : IQuery<EconomicActivityResponse>;