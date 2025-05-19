using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.EconomicActivities.CreateEconomicActivity;

public sealed record CreateEconomicActivityCommand(
    string EconomicActivityId,
    string Description,
    string CiiuCode,
    string DivisionCode,
    string DivisionName,
    string GroupName,
    string ClassCode,
    string StandardCode
) : ICommand<EconomicActivityResponse>;