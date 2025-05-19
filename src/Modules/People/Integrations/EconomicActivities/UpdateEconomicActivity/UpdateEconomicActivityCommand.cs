using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.EconomicActivities.UpdateEconomicActivity;

public sealed record UpdateEconomicActivityCommand(
    string EconomicActivityId,
    string NewEconomicActivityId,
    string NewDescription,
    string NewCiiuCode,
    string NewDivisionCode,
    string NewDivisionName,
    string NewGroupName,
    string NewClassCode,
    string NewStandardCode
) : ICommand<EconomicActivityResponse>;