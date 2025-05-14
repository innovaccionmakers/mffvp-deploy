using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.EconomicActivities.DeleteEconomicActivity;
public sealed record DeleteEconomicActivityCommand(
    string EconomicActivityId
) : ICommand;